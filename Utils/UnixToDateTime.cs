namespace TodoBackend.Utils;

public static class UnixToDateTime
{
    public static DateTime ReturnDateTime(long expiryTime)
    {
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(expiryTime).ToUniversalTime();
        return dateTime;
    }
}
