using CMetalsWS.Services.Dto;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
namespace CMetalsWS.Services
{
    public class PdfParsingService : IPdfParsingService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PdfParsingService> _logger;

        private const string MainPrompt = @"You are a structured data extractor for metal distribution picking lists.
You receive one or more PDF pages as images. Your job is to return only valid JSON that conforms to the schema below.

Key rules:
- Do not invent values.
- If a field is missing, return null or "".
- Output only JSON, no explanations or markdown.
- Parse numbers like ""2,024.344 LBS"" → 2024.344. Remove commas, units, and keep up to 3 decimals.

Fields to Extract:
- SalesOrderNumber: string
- OrderDate: YYYY-MM-DD or null
- ShipDate: YYYY-MM-DD or null
- SoldTo: string
- ShipTo: string
- SalesRep: string or null
- ShippingVia: string or null
- FOB: string or null
- Items: array of objects with fields:
  - LineNumber: integer or null
  - Quantity: number or null
  - ItemId: string or null
  - Description: string
  - Width: number or null, in inches
  - Length: number or null, in inches
  - Weight: number or null, in lbs
  - Uom: string or null
- Totals:
  - TotalWeightComputed: sum of all item Weight values (ignore nulls).
  - TotalWeightListed: the bottom-of-page grand total if present, else null.
  - TotalWeightDelta: TotalWeightComputed - (TotalWeightListed ?? 0).
  - TotalWeightMatch: true if totals exist and are within ±0.5% or ±5 lbs (whichever is greater). If no listed total, set true.

JSON Schema:
{
  ""type"": ""object"",
  ""properties"": {
    ""SalesOrderNumber"": { ""type"": ""string"" },
    ""OrderDate"": { ""type"": [""string"",""null""] },
    ""ShipDate"": { ""type"": [""string"",""null""] },
    ""SoldTo"": { ""type"": ""string"" },
    ""ShipTo"": { ""type"": ""string"" },
    ""SalesRep"": { ""type"": [""string"",""null""] },
    ""ShippingVia"": { ""type"": [""string"",""null""] },
    ""FOB"": { ""type"": [""string"",""null""] },
    ""Items"": {
      ""type"": ""array"",
      ""items"": {
        ""type"": ""object"",
        ""properties"": {
          ""LineNumber"": { ""type"": [""integer"",""null""] },
          ""Quantity"": { ""type"": [""number"",""null""] },
          ""ItemId"": { ""type"": [""string"",""null""] },
          ""Description"": { ""type"": ""string"" },
          ""Width"": { ""type"": [""number"",""null""] },
          ""Length"": { ""type"": [""number"",""null""] },
          ""Weight"": { ""type"": [""number"",""null""] },
          ""Uom"": { ""type"": [""string"",""null""] }
        },
        ""required"": [""Description""]
      }
    },
    ""TotalWeightComputed"": { ""type"": ""number"" },
    ""TotalWeightListed"": { ""type"": [""number"",""null""] },
    ""TotalWeightDelta"": { ""type"": ""number"" },
    ""TotalWeightMatch"": { ""type"": ""boolean"" }
  },
  ""required"": [
    ""SalesOrderNumber"",""OrderDate"",""ShipDate"",
    ""SoldTo"",""ShipTo"",""Items"",
    ""TotalWeightComputed"",""TotalWeightListed"",
    ""TotalWeightDelta"",""TotalWeightMatch""
  ]
}

The BranchId is determined server-side and will be applied to the entity during DB save. Do not guess or output BranchId.
Return exactly one JSON object that conforms to the schema above. No markdown, no comments, no explanations.";

        public PdfParsingService(IConfiguration configuration, ILogger<PdfParsingService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<PickingListExtraction?> ParsePdfAsync(Stream pdfStream)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogError("OpenAI API key is not configured in appsettings.json (OpenAI:ApiKey)");
                throw new InvalidOperationException("OpenAI API key is not configured.");
            }

            try
            {
                var images = await ConvertPdfToImages(pdfStream);
                var client = new ChatClient("gpt-4-vision-preview", apiKey);

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(MainPrompt)
                };

                var imageContentParts = images.Select(imageBytes =>
                {
                    var base64Image = Convert.ToBase64String(imageBytes);
                    return ChatMessageContentPart.CreateImagePart(new Uri($"data:image/png;base64,{base64Image}"));
                }).ToList();

                messages.Add(new UserChatMessage(imageContentParts));

                ChatCompletion completion = await client.CompleteChatAsync(messages);

                var jsonResponse = completion.Content[0].Text;
                _logger.LogInformation("OpenAI Response: {JsonResponse}", jsonResponse);

                return JsonSerializer.Deserialize<PickingListExtraction>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing PDF with OpenAI.");
                throw;
            }
        }

        private async Task<List<byte[]>> ConvertPdfToImages(Stream pdfStream)
        {
            var images = new List<byte[]>();
            await using var ms = new MemoryStream();
            await pdfStream.CopyToAsync(ms);
            ms.Position = 0;

            var imageStreams = PDFtoImage.Conversion.ToImages(ms);

            foreach (var image in imageStreams)
            {
                using var imageMemoryStream = new MemoryStream();
                image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(imageMemoryStream);
                images.Add(imageMemoryStream.ToArray());
                image.Dispose();
            }

            return images;
        }
    }
}
