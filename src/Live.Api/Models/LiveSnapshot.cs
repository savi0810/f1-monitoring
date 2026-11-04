namespace Live.Api.Models;

public class LiveSnapshot
{
    public int SessionKey { get; set; }
    public long Version { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<LiveDriver> Drivers { get; set; } = [];
}
