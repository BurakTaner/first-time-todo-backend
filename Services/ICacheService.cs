namespace TodoBackend.Services;

public interface ICacheService
{
    T Get<T>(string key);
    bool Set<T>(string key, T value, DateTimeOffset expTime);
    bool Delete(string key);
}
