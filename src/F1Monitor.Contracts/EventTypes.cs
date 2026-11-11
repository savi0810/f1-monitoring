namespace F1Monitor.Contracts;

public static class EventTypes
{
    public const string AlertTriggered = "alert_triggered";
    public const string PositionAlertTriggered = "position_alert_triggered";
    public const string LapCompleted = "lap_completed";
    public const string LiveSnapshot = "snapshot";
    public const string DriverPositionsChanged = "driver_positions_changed";
    public const string RaceCompleted = "race_completed";
    public const string SessionDriver = "session_driver";
    public const string SessionImportFinished = "session_import_finished";
}
