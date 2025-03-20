using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

namespace IntegrationTest.Utils;

public static class NetworkResolverUtils
{
    public static bool IsLocalNetwork()
    {
        return Environment.GetEnvironmentVariable("AGENT_CONTAINERNETWORK") == null;
    }

    public static string GetNetwork()
    {
        string network = Environment.GetEnvironmentVariable("AGENT_CONTAINERNETWORK")
                      ?? CreateNetworkAsync().GetAwaiter().GetResult();

        return network;
    }

    private static async Task<string> CreateNetworkAsync()
    {
        string networkName = Constants.DefaultNetworkName + TestDataUtils.GenerateUniqueValue();

        try
        {
            await new NetworkBuilder()
                .WithCleanUp(true)
                //.WithDriver(NetworkDriver.Bridge)
                .WithName(networkName)
                .Build()
                .CreateAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }


        return networkName;
    }
}
