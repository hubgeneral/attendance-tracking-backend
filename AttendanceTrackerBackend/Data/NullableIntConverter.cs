using System.Text.Json;
using System.Text.Json.Serialization;

public class NullableIntConverter : JsonConverter<int?>
{
    public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (string.IsNullOrEmpty(stringValue))
            {
                return null; // Handles ""
            }
            if (int.TryParse(stringValue, out int value))
            {
                return value; // Handles "4"
            }
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt32(out int value))
            {
                return value; // Handles 4
            }
        }
        else if (reader.TokenType == JsonTokenType.Null)
        {
            return null; // Handles null
        }

        // For floating point numbers, handle them as a string and parse.
        if (reader.TryGetDecimal(out var decimalValue))
        {
            return (int)decimalValue;
        }
        // Throw an exception for genuinely unsupported formats
        throw new JsonException($"The JSON value for '{reader.GetString()}' could not be converted to System.Nullable<Int32>.");
    }

    public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
          writer.WriteNumberValue(value.Value);
        }
        else
        {
          writer.WriteNullValue();
        }
    }
}