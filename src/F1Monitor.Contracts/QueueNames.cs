namespace F1Monitor.Contracts;

public static class QueueNames
{
    public const string TelemetryEvents = "telemetry.events";
    public const string AlertsEvents = "alerts.events";
    public const string LiveSnapshots = "live.snapshots";
    public const string AlertsLiveEvents = "alerts.live.events";
    public const string AlertTriggered = "alert.triggered";
    public const string PositionAlertsTriggered = "alerts.position_alerts_triggered";
    public const string SessionsAvailable = "sessions.available";
    public const string ImportRequested = "import.requested";
}
