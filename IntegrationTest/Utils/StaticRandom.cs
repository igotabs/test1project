using System.Diagnostics.CodeAnalysis;

namespace IntegrationTest.Utils;

[ExcludeFromCodeCoverage]
public static class StaticRandom
{
    private static readonly Random Getrandom = new();

    public static int GetRandomNumber(int min = 0, int max = 1000000)
    {
        lock (Getrandom)
        {
            return Getrandom.Next(min, max);
        }
    }

    public static double GetRandomDouble(int min = 0, int max = 1000, int precision = 5)
    {
        lock (Getrandom)
        {
            var value = (Getrandom.NextDouble() * (max - min)) + min;
            return Math.Truncate(value * Math.Pow(10, precision)) / Math.Pow(10, precision);
        }
    }

    public static bool GetRandomBoolean()
    {
        lock (Getrandom)
        {
            return Getrandom.Next(2) == 1;
        }
    }

    public static string GetString() => GetString(8).ToLowerInvariant();

    public static string GetString(int length)
    {
        return new string(Enumerable.Range(0, length)
        .Select(_ => (char)('A' + new Random().Next(0, 26))) // Generates uppercase letters
        .ToArray());
    }

    public static DateTime GetRandomDateTime()
    {
        int year = Getrandom.Next(1900, DateTime.Now.Year);
        int month = Getrandom.Next(1, 13);
        int day = Getrandom.Next(1, DateTime.DaysInMonth(year, month) + 1);
        int hour = Getrandom.Next(0, 24);
        int minute = Getrandom.Next(0, 60);
        int second = Getrandom.Next(0, 60);

        return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
    }


}
