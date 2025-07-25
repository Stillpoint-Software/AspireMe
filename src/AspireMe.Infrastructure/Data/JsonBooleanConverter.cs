using System.Text.Json;
using System.Text.Json.Serialization;

namespace AspireMe.Infrastructure.Data
{
    public class JsonBoolConverter : JsonConverter<bool>
    {
        public override void Write( Utf8JsonWriter writer, bool value, JsonSerializerOptions options ) =>
            writer.WriteBooleanValue( value );

        public override bool Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options ) =>
            reader.TokenType switch
            {
                JsonTokenType.True => true,
                JsonTokenType.False => false,
                JsonTokenType.String => bool.TryParse( reader.GetString(), out var b ) ? b : throw new JsonException(),
                _ => throw new JsonException(),
            };
    }
}
