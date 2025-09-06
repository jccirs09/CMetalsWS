#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CMetalsWS.Data;
using CMetalsWS.Services.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PDFtoImage;
using SkiaSharp;

namespace CMetalsWS.Services
{
    public class PdfParsingService : IPdfParsingService
    {
        private readonly ILogger<PdfParsingService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        static PdfParsingService()
        {
            _jsonOptions.Converters.Add(new FlexibleDateTimeConverter());
        }

        public PdfParsingService(ILogger<PdfParsingService> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<List<string>> ConvertPdfToImagesAsync(string sourcePdfPath, Guid importGuid)
        {
            var fileInfo = new FileInfo(sourcePdfPath);
            _logger.LogInformation("Reading PDF ({Len} bytes) from {Path} for conversion.", fileInfo.Length, sourcePdfPath);
            var pdfBytes = await File.ReadAllBytesAsync(sourcePdfPath);
            return await ConvertPdfToImagesAsync(pdfBytes, importGuid);
        }

        public async Task<List<string>> ConvertPdfToImagesAsync(byte[] pdfBytes, Guid importGuid)
        {
            var imagePaths = new List<string>();
            var outputDirectory = Path.Combine("wwwroot", "uploads", "pickinglists", importGuid.ToString());
            Directory.CreateDirectory(outputDirectory);

            try
            {
                _logger.LogInformation("Rendering PDF ({Len} bytes) to images.", pdfBytes.Length);
                var images = await Task.Run(() => Conversion.ToImages(pdfBytes));

                int pageNum = 1;
                foreach (var image in images.Take(5)) // Cap at 5 pages
                {
                    var imagePath = Path.Combine(outputDirectory, $"page-{pageNum}.jpeg");
                    bool success = await ProcessAndSaveImageAsync(image, imagePath, pageNum);
                    if (success)
                    {
                        imagePaths.Add(imagePath);
                    }
                    pageNum++;
                }

                _logger.LogInformation("Successfully processed and saved {PageCount} pages to {Directory}", imagePaths.Count, outputDirectory);
                return imagePaths;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to convert PDF byte array to images.");
                throw;
            }
        }

        private async Task<bool> ProcessAndSaveImageAsync(SKBitmap src, string outputPath, int pageNum)
        {
            const int maxDimension = 1600;
            const long twoMB = 2 * 1024 * 1024;

            using var original = src; // own the incoming bitmap exactly once
            SKBitmap? resized = null; // only dispose if we allocate it
            SKBitmap img = original;

            try
            {
                // Downscale if needed
                if (original.Width > maxDimension || original.Height > maxDimension)
                {
                    var ratio = (double)maxDimension / Math.Max(original.Width, original.Height);
                    var w = Math.Max(1, (int)Math.Round(original.Width * ratio));
                    var h = Math.Max(1, (int)Math.Round(original.Height * ratio));

                    // Prefer Resize (higher quality than ScalePixels on some builds)
                    resized = original.Resize(new SKImageInfo(w, h), SKFilterQuality.High);
                    if (resized != null)
                    {
                        img = resized;
                        _logger.LogInformation("Downscaled page {PageNum} {OrigW}x{OrigH} → {W}x{H}",
                            pageNum, original.Width, original.Height, w, h);
                    }
                    else
                    {
                        _logger.LogWarning("Resize failed on page {PageNum}; using original size.", pageNum);
                    }
                }

                // Encode via SKImage (more consistent than SKBitmap.Encode)
                using var ms = new MemoryStream();
                EncodeJpeg(img, ms, quality: 80);
                _logger.LogInformation("Page {PageNum} @q80 = {Bytes} bytes", pageNum, ms.Length);

                if (ms.Length > twoMB)
                {
                    _logger.LogWarning("Page {PageNum} too large at q80; re-encoding q70.", pageNum);
                    ms.SetLength(0);
                    EncodeJpeg(img, ms, quality: 70);
                    _logger.LogInformation("Page {PageNum} @q70 = {Bytes} bytes", pageNum, ms.Length);
                }

                if (ms.Length > twoMB)
                {
                    _logger.LogError("Page {PageNum} still >2MB after recompress; skipping.", pageNum);
                    return false;
                }

                await File.WriteAllBytesAsync(outputPath, ms.ToArray());
                return true;
            }
            finally
            {
                resized?.Dispose();
            }

            static void EncodeJpeg(SKBitmap bmp, Stream dest, int quality)
            {
                using var img = SKImage.FromBitmap(bmp);
                using var data = img.Encode(SKEncodedImageFormat.Jpeg, quality);
                data.SaveTo(dest);
            }
        }

        public async Task<(PickingList, List<PickingListItem>)> ParsePickingListAsync(IEnumerable<string> imagePaths)
        {
            if (!imagePaths.Any())
            {
                throw new ArgumentException("At least one image path is required for parsing.", nameof(imagePaths));
            }

            var header = await ParseHeaderAsync(imagePaths.First());
            var lineItems = await ParseLineItemsAsync(imagePaths);

            return (header, lineItems);
        }

        private async Task<PickingList> ParseHeaderAsync(string imagePath)
        {
            var prompt = @"Return ONLY a JSON object with the fields: { ""salesOrderNumber"": string, ""orderDate"": ""yyyy-MM-dd"", ""shipDate"": ""yyyy-MM-dd"", ""soldTo"": string, ""shipTo"": string, ""salesRep"": string, ""shippingVia"": string, ""fob"": string, ""buyer"": string|null, ""printDateTime"": ""yyyy-MM-dd HH:mm:ss"", ""totalWeight"": number }. Use exactly those formats. If seconds are unknown, use :00.";
            var json = await GetJsonFromVisionAsync(new[] { imagePath }, prompt, enforceJsonObject: true);

            var parsedHeader = JsonSerializer.Deserialize<PickingListHeaderDto>(json, _jsonOptions);
            if (parsedHeader == null)
            {
                throw new InvalidOperationException("Failed to deserialize header from OpenAI response.");
            }

            return new PickingList
            {
                SalesOrderNumber = parsedHeader.SalesOrderNumber,
                OrderDate = parsedHeader.OrderDate,
                ShipDate = parsedHeader.ShipDate,
                SoldTo = parsedHeader.SoldTo,
                ShipTo = parsedHeader.ShipTo,
                SalesRep = parsedHeader.SalesRep,
                ShippingVia = parsedHeader.ShippingVia,
                FOB = parsedHeader.FOB,
                Buyer = parsedHeader.Buyer,
                PrintDateTime = parsedHeader.PrintDateTime,
                TotalWeight = parsedHeader.TotalWeight
            };
        }

        private async Task<List<PickingListItem>> ParseLineItemsAsync(IEnumerable<string> imagePaths)
        {
            var prompt = @"Return ONLY a JSON array of line items: [ { ""lineNumber"": int, ""quantity"": number, ""itemId"": string, ""itemDescription"": string, ""itemDescription"": string, ""width"": number|string, ""length"": number|string, ""weight"": number, ""unit"": string } ]";
            var json = await GetJsonFromVisionAsync(imagePaths, prompt);

            var dtos = JsonSerializer.Deserialize<List<PickingListItemDto>>(json, _jsonOptions);
            if (dtos == null)
            {
                return new List<PickingListItem>();
            }

            return dtos.Select(dto => new PickingListItem
            {
                LineNumber = dto.LineNumber,
                Quantity = dto.Quantity,
                ItemId = dto.ItemId,
                ItemDescription = dto.ItemDescription,
                Width = NormalizeDimension(dto.Width),
                Length = NormalizeDimension(dto.Length),
                Weight = dto.Weight,
                Unit = string.IsNullOrWhiteSpace(dto.Unit) ? "EA" : dto.Unit
            }).ToList();
        }

        private async Task<string> GetJsonFromVisionAsync(IEnumerable<string> imagePaths, string prompt, bool enforceJsonObject = false)
        {
            var paths = imagePaths.Where(File.Exists).Take(5).ToList();
            if (paths.Count == 0) throw new FileNotFoundException("No valid image paths provided for vision parsing.");

            var apiKey = _configuration["OpenAI:ApiKey"];
            var model = _configuration["OpenAI:Model"] ?? "gpt-4o-mini";

            var contentParts = new List<OpenAiContentPart> { new OpenAiTextContentPart("text", prompt) };
            foreach (var path in paths)
            {
                var dataUrl = await BuildDataUrlAsync(path);
                contentParts.Add(new OpenAiImageUrlContentPart("image_url", new OpenAiImageUrl(dataUrl)));
            }

            var messages = new object[]
            {
                new { role = "system", content = "Return strictly valid JSON. No prose." },
                new { role = "user", content = contentParts }
            };

            var payload = new Dictionary<string, object>
            {
                { "model", model },
                { "messages", messages },
                { "temperature", 0 },
                { "max_tokens", 4000 }
            };

            if (enforceJsonObject)
            {
                payload["response_format"] = new { type = "json_object" };
            }

            var httpClient = _httpClientFactory.CreateClient("OpenAI");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var requestBody = JsonSerializer.Serialize(payload);
            _logger.LogDebug("OpenAI payload bytes: {Len}", Encoding.UTF8.GetByteCount(requestBody));
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("v1/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("OpenAI API request failed with status {StatusCode}: {ErrorBody}", response.StatusCode, errorBody);
                response.EnsureSuccessStatusCode(); // Throws HttpRequestException
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var openAiResponse = JsonSerializer.Deserialize<OpenAiResponse>(responseBody);

            var text = openAiResponse?.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? "";
            var json = TrimToJson(text);

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidOperationException("Model did not return valid JSON.");
            }

            return json;
        }

        private async Task<string> BuildDataUrlAsync(string path)
        {
            var bytes = await File.ReadAllBytesAsync(path);
            var b64 = Convert.ToBase64String(bytes);
            return $"data:image/jpeg;base64,{b64}";
        }

        // DTOs for serializing the request and deserializing the raw OpenAI API response
        [JsonDerivedType(typeof(OpenAiTextContentPart))]
        [JsonDerivedType(typeof(OpenAiImageUrlContentPart))]
        private abstract record OpenAiContentPart([property: JsonPropertyName("type")] string Type);
        private record OpenAiTextContentPart(string Type, [property: JsonPropertyName("text")] string Text) : OpenAiContentPart(Type);
        private record OpenAiImageUrlContentPart(string Type, [property: JsonPropertyName("image_url")] OpenAiImageUrl ImageUrl) : OpenAiContentPart(Type);
        private record OpenAiImageUrl([property: JsonPropertyName("url")] string Url);

        private record OpenAiResponse([property: JsonPropertyName("choices")] List<OpenAiChoice> Choices);
        private record OpenAiChoice([property: JsonPropertyName("message")] OpenAiMessage Message);
        private record OpenAiMessage([property: JsonPropertyName("content")] string Content);

        private string TrimToJson(string s)
        {
            int objStart = s.IndexOf('{');
            int objEnd = s.LastIndexOf('}');
            int arrStart = s.IndexOf('[');
            int arrEnd = s.LastIndexOf(']');

            if (arrStart >= 0 && arrEnd > arrStart)
            {
                return s.Substring(arrStart, arrEnd - arrStart + 1);
            }

            if (objStart >= 0 && objEnd > objStart)
            {
                return s.Substring(objStart, objEnd - objStart + 1);
            }

            throw new InvalidOperationException("No JSON found in model output.");
        }

        private decimal? NormalizeDimension(object? dimension)
        {
            if (dimension == null) return null;

            if (dimension is decimal d) return d;
            if (dimension is JsonElement je)
            {
                if (je.ValueKind == JsonValueKind.Number) return je.GetDecimal();
                if (je.ValueKind == JsonValueKind.String)
                {
                    var s = je.GetString();
                    if (s == null) return null;
                    return ParseDimensionString(s);
                }
            }

            return ParseDimensionString(dimension.ToString());
        }

        private decimal? ParseDimensionString(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;

            var cleaned = s
              .Replace("\"", "")
              .Replace("inch", "", StringComparison.OrdinalIgnoreCase)
              .Replace("in.", "", StringComparison.OrdinalIgnoreCase)
              .Replace("in", "", StringComparison.OrdinalIgnoreCase)
              .Replace("-", " ")
              .Trim();

            // Remove thousands separators, keep dot for decimals
            cleaned = cleaned.Replace(",", "");

            return decimal.TryParse(cleaned, System.Globalization.NumberStyles.Float,
                                    System.Globalization.CultureInfo.InvariantCulture, out var d)
                   ? Math.Round(d, 3)
                   : (decimal?)null;
        }
    }
}
