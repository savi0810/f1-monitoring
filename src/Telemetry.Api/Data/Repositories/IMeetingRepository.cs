using Telemetry.Api.Data.Models;

namespace Telemetry.Api.Data.Repositories;

public interface IMeetingRepository
{
    Task UpsertMeetingAsync(Meeting meeting);
}
