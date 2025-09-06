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
                    var imagePath = Path.Combine(outputDirectory, $"page-{pageNum}.png");
                    using (var stream = File.Create(imagePath))
                    {
                        image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(stream);
                    }
                    imagePaths.Add(imagePath);
                    pageNum++;
                }

                _logger.LogInformation("Successfully converted PDF to {PageCount} images in {Directory}", imagePaths.Count, outputDirectory);
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

            _logger.LogInformation("Using OpenAI model: {Model}", _chatClient.Model);

            var pickingList = await ParseHeaderAsync(imagePaths.First());
            var dtoList = await ParseLineItemsAsync(imagePaths);
            var pickingListItems = NormalizePickingListItems(dtoList);

            return (pickingList, pickingListItems);
        }

        private async Task<List<PickingListItemDto>> ParseLineItemsAsync(IEnumerable<string> imagePaths)
        {
            var allItems = new List<PickingListItemDto>();
            var pageNum = 1;

            foreach (var imagePath in imagePaths)
            {
                _logger.LogInformation("Parsing line items from image: {ImagePath} (Page {PageNum})", imagePath, pageNum);

                var prompt = @"Analyze the provided picking list image and extract all line items into a valid JSON array. Each object in the array should follow this structure:
{
  ""LineNumber"": ""integer"",
  ""Quantity"": ""decimal"",
  ""ItemId"": ""string"",
  ""ItemDescription"": ""string"",
  ""Width"": ""decimal | string"",
  ""Length"": ""decimal | string"",
  ""Weight"": ""decimal"",
  ""Unit"": ""string (e.g., EA, FT, LB)""
}
Ensure that width and length values with inch marks (e.g., 60"") are preserved as strings. Default 'Unit' to 'EA' if it's not present. Respond with only the JSON array.";

                var imageBytes = File.ReadAllBytes(imagePath);
                var base64Image = Convert.ToBase64String(imageBytes);
                var dataUri = $"data:image/png;base64,{base64Image}";

                var messages = new List<OpenAI.Chat.ChatMessage>
                {
                    new SystemChatMessage("You are an intelligent assistant that extracts structured data from documents. Your task is to return a JSON array of line item objects. Do not include any explanatory text, just the JSON array. If there are no line items on the page, return an empty array []."),
                    new UserChatMessage(new List<ChatMessageContentPart>
                    {
                        ChatMessageContentPart.CreateTextPart(prompt),
                        ChatMessageContentPart.CreateImagePart(new Uri(dataUri))
                    })
                };

                try
                {
                    ChatCompletion completion = await _chatClient.CompleteChatAsync(messages);

                    var jsonResponse = completion.Content[0].Text;
                    _logger.LogDebug("OpenAI raw response for line items (Page {PageNum}): {Response}", pageNum, jsonResponse);

                    var items = JsonSerializer.Deserialize<List<PickingListItemDto>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (items != null)
                    {
                        allItems.AddRange(items);
                        _logger.LogInformation("Successfully parsed {ItemCount} line items from page {PageNum}", items.Count, pageNum);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse picking list line items from image {ImagePath}", imagePath);
                }
                pageNum++;
            }

            return allItems;
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

        private async Task<PickingList> ParseHeaderAsync(string imagePath)
        {
            _logger.LogInformation("Parsing header from image: {ImagePath}", imagePath);

            var prompt = @"Analyze the provided picking list image and extract the header information into a valid JSON object. The structure should be:
{
  ""SalesOrderNumber"": ""string"",
  ""OrderDate"": ""YYYY-MM-DD"",
  ""ShipDate"": ""YYYY-MM-DD"",
  ""SoldTo"": ""string"",
  ""ShipTo"": ""string"",
  ""SalesRep"": ""string"",
  ""ShippingVia"": ""string"",
  ""FOB"": ""string"",
  ""Buyer"": ""string | null"",
  ""PrintDateTime"": ""YYYY-MM-DD HH:mm:ss"",
  ""TotalWeight"": ""decimal""
}
Respond with only the JSON object.";

            var imageBytes = File.ReadAllBytes(imagePath);
            var base64Image = Convert.ToBase64String(imageBytes);
            var dataUri = $"data:image/png;base64,{base64Image}";

            var messages = new List<OpenAI.Chat.ChatMessage>
            {
                new SystemChatMessage("You are an intelligent assistant that extracts structured data from documents. Your task is to return a JSON object with the extracted header data. Do not include any explanatory text, just the JSON."),
                new UserChatMessage(new List<ChatMessageContentPart>
                {
                    ChatMessageContentPart.CreateTextPart(prompt),
                    ChatMessageContentPart.CreateImagePart(new Uri(dataUri))
                })
            };

            try
            {
                ChatCompletion completion = await _chatClient.CompleteChatAsync(messages);

                var jsonResponse = completion.Content[0].Text;
                _logger.LogDebug("OpenAI raw response for header: {Response}", jsonResponse);

                var parsedHeader = JsonSerializer.Deserialize<PickingList>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (parsedHeader == null)
                {
                    throw new InvalidOperationException("Failed to deserialize header JSON from OpenAI response.");
                }

                _logger.LogInformation("Successfully parsed header for SO: {SalesOrderNumber}", parsedHeader.SalesOrderNumber);
                return parsedHeader;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse picking list header from image {ImagePath}", imagePath);
                throw;
            }
        }
    }
}
