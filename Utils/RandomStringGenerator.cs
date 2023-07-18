namespace TodoBackend.Utils;

public static class RandomStringGenerator
{
    public static string GenerateRandomString(int strLen)
    {
        Random random = new();
        string chars = "ABCÇDEFGĞHIİJKLMNOÖPRSŞTUÜVYZ1234567890abcçdefgğhıijklmnoöprsştuüvyz";
        return new string(Enumerable.Repeat(chars, strLen).Select(a => a[random.Next(a.Length)]).ToArray());
    }
}
