using System.Text.Json;
using System.Text.Json.Serialization;

namespace T_Badge.Contracts.Converters;

public class UnixEpochConverter : JsonConverter<DateTime>
{
    public override DateTime Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var unixTime = reader.GetInt64();
        var dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime;

        return dateTime;
    }

    public override void Write(
        Utf8JsonWriter writer,
        DateTime value,
        JsonSerializerOptions options)
    {
        var unixTime = new DateTimeOffset(value).ToUnixTimeSeconds();
        writer.WriteNumberValue(unixTime);
    }
}