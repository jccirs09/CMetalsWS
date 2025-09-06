#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
using CMetalsWS.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using OpenAI;
using OpenAI.Chat;
using PDFtoImage;
using SkiaSharp;
using System.Linq;
using System.Text.RegularExpressions;

namespace CMetalsWS.Services
{
    public class PdfParsingService : IPdfParsingService
    {
        private readonly ILogger<PdfParsingService> _logger;
        private readonly ChatClient _chatClient;

        public PdfParsingService(ILogger<PdfParsingService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _chatClient = new ChatClient(
                model: configuration.GetValue<string>("OpenAI:Model") ?? "gpt-4o-mini",
                apiKey: configuration.GetValue<string>("OpenAI:ApiKey")
            );
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
                foreach (var image in images.Take(5)) // Cap at 5 pages to prevent giant payloads
                {
                    using var img = image; // Dispose Skia resource
                    var imagePath = Path.Combine(outputDirectory, $"page-{pageNum}.jpeg");
                    using (var stream = File.Create(imagePath))
                    {
                        img.Encode(SKEncodedImageFormat.Jpeg, 85).SaveTo(stream);
                    }
                    imagePaths.Add(imagePath);
                    pageNum++;
                }
                _logger.LogInformation("Successfully converted PDF to {PageCount} JPEG images in {Directory}", imagePaths.Count, outputDirectory);
                return imagePaths;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to convert PDF byte array to images.");
                throw;
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

            var contentParts = new List<ChatMessageContentPart> { ChatMessageContentPart.CreateTextPart(prompt) };
            foreach (var path in paths)
            {
                var dataUrl = await ToDataUrlAsync(path);
                contentParts.Add(ChatMessageContentPart.CreateImagePart(new Uri(dataUrl)));
            }

            var messages = new List<OpenAI.Chat.ChatMessage>
            {
                new SystemChatMessage("Return strictly valid JSON. No prose."),
                new UserChatMessage(contentParts)
            };

            var options = new ChatCompletionOptions { Temperature = 0 };
            var completion = await _chatClient.CompleteChatAsync(messages, options);

            // Reverting to .Value.Content as the compiler requires it.
            var text = completion.Value.Content.FirstOrDefault()?.Text?.Trim() ?? "";
            var json = TrimToJson(text);

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidOperationException("Model did not return valid JSON.");
            }

            return json;
        }

        private async Task<string> ToDataUrlAsync(string path)
        {
            var bytes = await File.ReadAllBytesAsync(path);
            var b64 = Convert.ToBase64String(bytes);
            return $"data:image/jpeg;base64,{b64}";
        }

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
