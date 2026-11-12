namespace F1Monitor.Contracts;

public static class RoutingKeys
{
    public const string Live = "live";
    public const string History = "history";
    public const string Alerts = "alerts";

    public const string AllEvents = "#";

    public static string For(string source, string eventType) => $"{source}.{eventType}";
}
