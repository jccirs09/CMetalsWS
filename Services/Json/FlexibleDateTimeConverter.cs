using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CMetalsWS.Services.Json
{
    public sealed class FlexibleDateTimeConverter : JsonConverter<DateTime?>
    {
        private static readonly string[] DateTimeFormats = new[]
        {
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss",
            "MM/dd/yyyy h:mm:ss tt",
            "M/d/yyyy h:mm:ss tt",
            "yyyy-MM-dd",
            "MM/dd/yyyy",
            "M/d/yyyy"
        };

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();
                if (string.IsNullOrWhiteSpace(s))
                {
                    return null;
                }

                if (DateTime.TryParseExact(s.Trim(), DateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                {
                    return dt;
                }

                // Last-ditch attempt for any other format
                if (DateTime.TryParse(s, out dt))
                {
                    return dt;
                }
            }

            // Per feedback, changed from throwing exception to returning null to be more lenient.
            // A null return will be handled by the service logic.
            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture));
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
