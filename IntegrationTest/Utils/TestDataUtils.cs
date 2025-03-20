namespace IntegrationTest.Utils;

public static class TestDataUtils
{
    public static string GenerateUniqueValue()
    {
        return Guid.NewGuid().ToString("N");
    }

    public static string GetTodayDate()
    {
        return DateTime.Today.ToString("ddMMyyyy");
    }

    public static long GenerateTimestamp()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

}
