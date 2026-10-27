using System.Security.Cryptography;
using System.Text;

namespace F1.Collector.Mapping;

public static class EventIdFactory
{
    public static Guid Create(string key)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));

        return new Guid(hash.AsSpan(0, 16));
    }
}
