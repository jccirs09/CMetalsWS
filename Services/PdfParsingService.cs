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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CMetalsWS.Data;
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

        private async Task<bool> ProcessAndSaveImageAsync(SKBitmap originalImage, string outputPath, int pageNum)
        {
            const int maxDimension = 1600;
            const long twoMegabytes = 2 * 1024 * 1024;

            using (originalImage)
            {
                // 1. Downscale the image if it's too large
                SKBitmap imageToEncode = originalImage;
                if (originalImage.Width > maxDimension || originalImage.Height > maxDimension)
                {
                    var ratio = (double)maxDimension / Math.Max(originalImage.Width, originalImage.Height);
                    var newWidth = (int)(originalImage.Width * ratio);
                    var newHeight = (int)(originalImage.Height * ratio);

                    var newBitmap = new SKBitmap(newWidth, newHeight);
                    if (originalImage.ScalePixels(newBitmap, SKFilterQuality.High))
                    {
                        imageToEncode = newBitmap;
                        _logger.LogInformation("Downscaled page {PageNum} from {OrigW}x{OrigH} to {NewW}x{NewH}", pageNum, originalImage.Width, originalImage.Height, newWidth, newHeight);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to downscale page {PageNum}, using original.", pageNum);
                        newBitmap.Dispose(); // Dispose the unused bitmap
                    }
                }

                using (imageToEncode) // Ensure the potentially new bitmap is disposed
                {
                    // 2. Dynamically adjust JPEG quality
                    using var ms = new MemoryStream();

                    // Try quality 80
                    imageToEncode.Encode(ms, SKEncodedImageFormat.Jpeg, 80);
                    _logger.LogInformation("Page {PageNum} encoded at quality 80 is {Size} bytes.", pageNum, ms.Length);

                    // If too large, try quality 70
                    if (ms.Length > twoMegabytes)
                    {
                        _logger.LogWarning("Page {PageNum} at quality 80 is too large ({Size} bytes). Re-encoding at quality 70.", pageNum, ms.Length);
                        ms.SetLength(0); // Reset stream
                        imageToEncode.Encode(ms, SKEncodedImageFormat.Jpeg, 70);
                        _logger.LogInformation("Page {PageNum} encoded at quality 70 is {Size} bytes.", pageNum, ms.Length);
                    }

                    // If still too large, skip the page
                    if (ms.Length > twoMegabytes)
                    {
                        _logger.LogError("Page {PageNum} is still too large ({Size} bytes) after re-compression. Skipping.", pageNum, ms.Length);
                        return false;
                    }

                    // 3. Save the final image to disk
                    await File.WriteAllBytesAsync(outputPath, ms.ToArray());
                    return true;
                }
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
            var prompt = @"Return ONLY a JSON object with: { ""salesOrderNumber"": ""string, from the 'Picking List No.' field"", ""orderDate"": ""YYYY-MM-DD"", ""shipDate"": ""YYYY-MM-DD"", ""soldTo"": ""string"", ""shipTo"": ""string"", ""salesRep"": ""string"", ""shippingVia"": ""string"", ""fob"": ""string"", ""totalWeight"": number, ""buyer"": ""string | null"", ""printDateTime"": ""YYYY-MM-DD HH:mm:ss"" }";
            var json = await GetJsonFromVisionAsync(new[] { imagePath }, prompt);

            var parsedHeader = JsonSerializer.Deserialize<PickingListHeaderDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
            var prompt = @"Return ONLY a JSON array of line items: [ { ""lineNumber"": int, ""quantity"": number, ""itemId"": string, ""itemDescription"": string, ""width"": number|string, ""length"": number|string, ""weight"": number, ""unit"": string } ]";
            var json = await GetJsonFromVisionAsync(imagePaths, prompt);

            var dtos = JsonSerializer.Deserialize<List<PickingListItemDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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

        private async Task<string> GetJsonFromVisionAsync(IEnumerable<string> imagePaths, string prompt)
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

            var payload = new
            {
                model,
                messages = new object[]
                {
                    new { role = "system", content = "Return strictly valid JSON. No prose." },
                    new { role = "user", content = contentParts }
                },
                temperature = 0
            };

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = JsonSerializer.Serialize(payload);
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

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
