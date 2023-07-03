using System.Text.Json;
using StackExchange.Redis;

namespace TodoBackend.Services;

public class CacheService : ICacheService
{
    private IDatabase _cache;

    public CacheService()
    {
        ConnectionMultiplexer connection = ConnectionMultiplexer.Connect("localhost:6379");
        _cache = connection.GetDatabase();
    }

    public bool Delete(string key)
    {
        bool _exists = _cache.KeyExists(key);
        if (_exists)
            return _cache.KeyDelete(key);
        else
            return false;
    }

    public T Get<T>(string key)
    {
        RedisValue value = _cache.StringGet(key);
        if (!String.IsNullOrWhiteSpace(value))
            return JsonSerializer.Deserialize<T>(value);
        else
            return default;
    }

    public bool Set<T>(string key, T value, DateTimeOffset expTime)
    {
        TimeSpan time = expTime.DateTime.Subtract(DateTime.Now);
        return _cache.StringSet(key, JsonSerializer.Serialize<T>(value), time);
    }
}
