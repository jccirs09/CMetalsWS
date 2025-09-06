using CMetalsWS.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace CMetalsWS.Services
{
    public class PdfParsingService : IPdfParsingService
    {
        private readonly ILogger<PdfParsingService> _logger;
        private readonly IConfiguration _configuration;
        private readonly OpenAIAPI _openAiApi;

        public PdfParsingService(ILogger<PdfParsingService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _openAiApi = new OpenAIAPI(new APIAuthentication(_configuration["OpenAI:ApiKey"]));
        }

        public async Task<List<string>> ConvertPdfToImagesAsync(string sourcePdfPath, Guid importGuid)
        {
            var imagePaths = new List<string>();
            var outputDirectory = Path.Combine("wwwroot", "uploads", "pickinglists", importGuid.ToString());
            Directory.CreateDirectory(outputDirectory);

            try
            {
                var dpi = _configuration.GetValue<int>("PdfToImage:Dpi", 300);

                // Using the PDFtoImage library
                var conversion = new PDFtoImage.Conversion
                {
                    DPI = dpi
                };

                var images = await Task.Run(() => conversion.ToImages(sourcePdfPath));

                int pageNum = 1;
                foreach (var image in images)
                {
                    var imagePath = Path.Combine(outputDirectory, $"page-{pageNum}.png");
                    await image.SaveAsync(imagePath);
                    imagePaths.Add(imagePath);
                    pageNum++;
                }

                _logger.LogInformation("Successfully converted PDF to {PageCount} images in {Directory}", images.Count(), outputDirectory);
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

            var model = _configuration["OpenAI:Model"] ?? "gpt-4o-mini";
            _logger.LogInformation("Using OpenAI model: {Model}", model);

            var pickingList = await ParseHeaderAsync(imagePaths.First(), model);
            var pickingListItems = await ParseLineItemsAsync(imagePaths, model);

            NormalizePickingListItems(pickingListItems);

            return (pickingList, pickingListItems);
        }

        private async Task<List<PickingListItem>> ParseLineItemsAsync(IEnumerable<string> imagePaths, string model)
        {
            var allItems = new List<PickingListItem>();
            var pageNum = 1;

            foreach (var imagePath in imagePaths)
            {
                _logger.LogInformation("Parsing line items from image: {ImagePath} (Page {PageNum})", imagePath, pageNum);

                var request = _openAiApi.Chat.CreateConversation();
                request.Model = new Model(model);

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
Ensure that width and length values with inch marks (e.g., 60"") are preserved as strings. Default 'Unit' to 'EA' if it's not present.";

                request.AppendSystemMessage("You are an intelligent assistant that extracts structured data from documents. The user will provide an image of a picking list page. Your task is to return a JSON array of line item objects. Do not include any explanatory text, just the JSON array. If there are no line items on the page, return an empty array [].");
                request.AppendUserInput(prompt, ImageInput.FromFile(imagePath));

                try
                {
                    var response = await request.GetResponseFromChatbotAsync();
                    _logger.LogDebug("OpenAI raw response for line items (Page {PageNum}): {Response}", pageNum, response);

                    var jsonResponse = response.Replace("```json", "").Replace("```", "").Trim();
                    var items = System.Text.Json.JsonSerializer.Deserialize<List<PickingListItem>>(jsonResponse, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (items != null)
                    {
                        allItems.AddRange(items);
                        _logger.LogInformation("Successfully parsed {ItemCount} line items from page {PageNum}", items.Count, pageNum);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse picking list line items from image {ImagePath}", imagePath);
                    // Continue to next page even if one fails
                }
                pageNum++;
            }

            return allItems;
        }

        private void NormalizePickingListItems(List<PickingListItem> items)
        {
            foreach (var item in items)
            {
                // Normalize Width
                if (item.Width is string widthStr && widthStr.EndsWith("\""))
                {
                    if (decimal.TryParse(widthStr.TrimEnd('"'), out var width))
                    {
                        item.Width = width;
                    }
                }

                // Normalize Length
                if (item.Length is string lengthStr && lengthStr.EndsWith("\""))
                {
                    if (decimal.TryParse(lengthStr.TrimEnd('"'), out var length))
                    {
                        item.Length = length;
                    }
                }

                // Default Unit
                if (string.IsNullOrWhiteSpace(item.Unit))
                {
                    item.Unit = "EA";
                }
            }
        }

        private async Task<PickingList> ParseHeaderAsync(string imagePath, string model)
        {
            _logger.LogInformation("Parsing header from image: {ImagePath}", imagePath);

            var request = _openAiApi.Chat.CreateConversation();
            request.Model = new Model(model);

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
}";

            request.AppendSystemMessage("You are an intelligent assistant that extracts structured data from documents. The user will provide an image of a picking list. Your task is to return a JSON object with the extracted header data. Do not include any explanatory text, just the JSON.");
            request.AppendUserInput(prompt, ImageInput.FromFile(imagePath));

            try
            {
                var response = await request.GetResponseFromChatbotAsync();
                _logger.LogDebug("OpenAI raw response for header: {Response}", response);

                var jsonResponse = response.Replace("```json", "").Replace("```", "").Trim();
                var parsedHeader = System.Text.Json.JsonSerializer.Deserialize<PickingList>(jsonResponse, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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
