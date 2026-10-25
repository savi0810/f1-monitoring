using System.Text.Json;

namespace F1Monitor.Contracts;

public static class EventSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}
