using F1.Collector.OpenF1;
using F1Monitor.Contracts;

namespace F1.Collector.Mapping;

public static class SessionAvailableMapper
{
    public static SessionAvailable Map(OpenF1Session session) => new(
        session.SessionKey,
        session.MeetingKey,
        session.Year,
        session.SessionType,
        session.SessionName,
        session.CircuitShortName,
        session.CountryName,
        session.DateStart,
        session.DateEnd,
        session.IsCancelled);
}
