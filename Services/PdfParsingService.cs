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

namespace CMetalsWS.Services
{
    public class PdfParsingService : IPdfParsingService
    {
        private readonly ILogger<PdfParsingService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ChatClient _chatClient;

        public PdfParsingService(ILogger<PdfParsingService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _chatClient = new ChatClient(
                model: configuration.GetValue<string>("OpenAI:Model") ?? "gpt-4o-mini",
                apiKey: configuration.GetValue<string>("OpenAI:ApiKey")
            );
        }

        public async Task<List<string>> ConvertPdfToImagesAsync(string sourcePdfPath, Guid importGuid)
        {
            var imagePaths = new List<string>();
            var outputDirectory = Path.Combine("wwwroot", "uploads", "pickinglists", importGuid.ToString());
            Directory.CreateDirectory(outputDirectory);

            try
            {
                var images = await Task.Run(() => Conversion.ToImages(sourcePdfPath));
                int pageNum = 1;
                foreach (var image in images)
                {
                    var imagePath = Path.Combine(outputDirectory, $"page-{pageNum}.jpeg");
                    using (var stream = File.Create(imagePath))
                    {
                        image.Encode(SKEncodedImageFormat.Jpeg, 85).SaveTo(stream);
                    }
                    imagePaths.Add(imagePath);
                    pageNum++;
                }

                _logger.LogInformation("Successfully converted PDF to {PageCount} JPEG images in {Directory}", imagePaths.Count, outputDirectory);
                return imagePaths;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to convert PDF to images for {PdfPath}", sourcePdfPath);
                throw;
            }
        }

        public async Task<(PickingList, List<PickingListItem>)> ParsePickingListAsync(IEnumerable<string> imagePaths)
        {
            if (!imagePaths.Any())
            {
                throw new ArgumentException("At least one image path is required for parsing.", nameof(imagePaths));
            }

            _logger.LogInformation("Starting batched parsing for {ImageCount} images.", imagePaths.Count());

            PickingList? finalHeader = null;
            var allItems = new List<PickingListItem>();
            var imageChunks = imagePaths.Select((path, index) => new { path, index }).GroupBy(x => x.index / 5).Select(g => g.Select(x => x.path).ToList()).ToList();

            bool isFirstChunk = true;

            foreach (var chunk in imageChunks)
            {
                var parsedResult = await ParseImageChunkAsync(chunk, isFirstChunk);
                if (isFirstChunk && parsedResult?.Header != null)
                {
                    var h = parsedResult.Header;
                    finalHeader = new PickingList
                    {
                        SalesOrderNumber = h.SalesOrderNumber,
                        OrderDate = h.OrderDate,
                        ShipDate = h.ShipDate,
                        SoldTo = h.SoldTo,
                        ShipTo = h.ShipTo,
                        SalesRep = h.SalesRep,
                        ShippingVia = h.ShippingVia,
                        FOB = h.FOB,
                        Buyer = h.Buyer,
                        PrintDateTime = h.PrintDateTime,
                        TotalWeight = h.TotalWeight
                    };
                }

                if (parsedResult?.LineItems != null)
                {
                    var normalizedItems = NormalizePickingListItems(parsedResult.LineItems);
                    allItems.AddRange(normalizedItems);
                }

                isFirstChunk = false;
            }

            if (finalHeader == null)
            {
                throw new InvalidOperationException("Failed to parse picking list header from the first page(s).");
            }

            return (finalHeader, allItems);
        }

        private async Task<ParsingResultDto?> ParseImageChunkAsync(List<string> imagePaths, bool isFirstChunk)
        {
             _logger.LogInformation("Parsing a chunk of {ImageCount} images.", imagePaths.Count);

            var prompt = @"Analyze the provided picking list image(s) and extract the header and line item information into a valid JSON object.
- The header information will only be on the first page.
- Line items may span multiple pages.
- If a field is not present, use null.
- Ensure that width and length values with inch marks (e.g., 60"") are preserved as strings.
- Default 'Unit' to 'EA' if it's not present.

The JSON structure should be:
{
  ""header"": {
    ""salesOrderNumber"": ""string, from the 'Picking List No.' field"",
    ""orderDate"": ""YYYY-MM-DD"",
    ""shipDate"": ""YYYY-MM-DD"",
    ""soldTo"": ""string"",
    ""shipTo"": ""string"",
    ""salesRep"": ""string"",
    ""shippingVia"": ""string"",
    ""fob"": ""string"",
    ""buyer"": ""string | null"",
    ""printDateTime"": ""YYYY-MM-DD HH:mm:ss"",
    ""totalWeight"": ""decimal""
  },
  ""lineItems"": [
    {
      ""lineNumber"": ""integer"",
      ""quantity"": ""decimal"",
      ""itemId"": ""string"",
      ""itemDescription"": ""string"",
      ""width"": ""decimal | string"",
      ""length"": ""decimal | string"",
      ""weight"": ""decimal"",
      ""unit"": ""string""
    }
  ]
}

If this is not the first page, the ""header"" field can be null.";

            var contentParts = new List<ChatMessageContentPart> { ChatMessageContentPart.CreateTextPart(prompt) };
            foreach(var imagePath in imagePaths)
            {
                var imageBytes = File.ReadAllBytes(imagePath);
                var dataUri = $"data:image/jpeg;base64,{Convert.ToBase64String(imageBytes)}";
                contentParts.Add(ChatMessageContentPart.CreateImagePart(new Uri(dataUri)));
            }

            var messages = new List<OpenAI.Chat.ChatMessage>
            {
                new SystemChatMessage("You are an intelligent assistant that extracts structured data from documents. Your task is to return a single JSON object. Do not include any explanatory text, just the JSON."),
                new UserChatMessage(contentParts)
            };

            try
            {
                ChatCompletion completion = await _chatClient.CompleteChatAsync(messages);
                var jsonResponse = completion.Content[0].Text;
                _logger.LogDebug("OpenAI raw response for chunk: {Response}", jsonResponse);
                return JsonSerializer.Deserialize<ParsingResultDto>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to parse image chunk.");
                return null;
            }
        }

        private List<PickingListItem> NormalizePickingListItems(List<PickingListItemDto> dtos)
        {
            var result = new List<PickingListItem>();
            foreach (var dto in dtos)
            {
                var item = new PickingListItem
                {
                    LineNumber = dto.LineNumber,
                    Quantity = dto.Quantity,
                    ItemId = dto.ItemId,
                    ItemDescription = dto.ItemDescription,
                    Weight = dto.Weight,
                    Unit = string.IsNullOrWhiteSpace(dto.Unit) ? "EA" : dto.Unit
                };

                if (dto.Width is JsonElement widthElement)
                {
                    if (widthElement.ValueKind == JsonValueKind.String)
                    {
                        var widthStr = widthElement.GetString() ?? "";
                        if (widthStr.EndsWith("\"") && decimal.TryParse(widthStr.TrimEnd('"'), out var width))
                        {
                            item.Width = width;
                        }
                    }
                    else if (widthElement.ValueKind == JsonValueKind.Number)
                    {
                        item.Width = widthElement.GetDecimal();
                    }
                }

                if (dto.Length is JsonElement lengthElement)
                {
                    if (lengthElement.ValueKind == JsonValueKind.String)
                    {
                        var lengthStr = lengthElement.GetString() ?? "";
                        if (lengthStr.EndsWith("\"") && decimal.TryParse(lengthStr.TrimEnd('"'), out var length))
                        {
                            item.Length = length;
                        }
                    }
                    else if (lengthElement.ValueKind == JsonValueKind.Number)
                    {
                        item.Length = lengthElement.GetDecimal();
                    }
                }

                result.Add(item);
            }
            return result;
        }
    }
}
