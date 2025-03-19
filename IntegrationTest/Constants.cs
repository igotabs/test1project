namespace IntegrationTest;

public static class Constants
{
    public const string NugetUrl =
        "https://pkgs.dev.azure.com/PG-NM/NM-ProgX/_packaging/NM-ProgX-Packages/nuget/v3/index.json";
    public const string ValidatorContainerName = "ValidatorContainerName";
    public const string DefaultNetworkName = "integration_test_network";
    public static readonly TimeSpan AssertTimeout = TimeSpan.FromSeconds(20);
}
