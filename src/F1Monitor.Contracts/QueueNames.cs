namespace F1Monitor.Contracts;

public static class QueueNames
{
    public const string F1Events = "f1.events";
    public const string TelemetryEvents = "telemetry.events";
    public const string AlertsEvents = "alerts.events";
    public const string LiveEvents = "live.events";
    public const string AlertTriggered = "alert.triggered";
    public const string SessionsAvailable = "sessions.available";
    public const string ImportRequested = "import.requested";
}
