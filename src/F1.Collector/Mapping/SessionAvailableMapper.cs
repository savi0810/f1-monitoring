using F1.Collector.OpenF1;
using F1Monitor.Contracts;

namespace F1.Collector.Mapping;

public static class SessionAvailableMapper
{
    public static SessionAvailable Map(OpenF1Session session, OpenF1Meeting? meeting) => new(
        session.SessionKey,
        session.MeetingKey,
        meeting?.MeetingName,
        session.Year,
        session.SessionType,
        session.SessionName,
        session.CircuitShortName,
        session.CountryName,
        session.DateStart,
        session.DateEnd,
        session.IsCancelled);
}
