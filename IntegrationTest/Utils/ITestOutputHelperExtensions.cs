using Xunit.Abstractions;

namespace IntegrationTest.Utils;

public static class ITestOutputHelperExtensions
{
    public static void Log(this ITestOutputHelper testOutputHelper, string message)
    {
        testOutputHelper.WriteLine("[{0:yyyy/MM/dd hh:mm:ss:ffff}][ServiceTests] {1}", DateTime.Now, message);
    }
}
