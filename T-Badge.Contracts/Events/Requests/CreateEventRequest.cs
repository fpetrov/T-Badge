using System.Text.Json.Serialization;
using T_Badge.Contracts.Converters;

namespace T_Badge.Contracts.Events.Requests;

public record CreateEventRequest(
    string Title,
    string Description,
    string Location,
    [property: JsonConverter(typeof(UnixEpochConverter))] DateTime Start,
    [property: JsonConverter(typeof(UnixEpochConverter))] DateTime End);