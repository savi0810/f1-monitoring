namespace F1Monitor.Contracts;

public static class QueueNames
{
    public const string TelemetryEvents = "telemetry.events";
    public const string AlertsEvents = "alerts.events";
    public const string LiveSnapshots = "live.snapshots";
    public const string AlertsLiveSnapshots = "alerts.live.snapshots";
    public const string AlertTriggered = "alert.triggered";
    public const string SessionsAvailable = "sessions.available";
    public const string ImportRequested = "import.requested";
}
