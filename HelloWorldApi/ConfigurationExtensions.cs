namespace Microsoft.Extensions.Configuration;

public static class ConfigurationExtensions
{
    public static T GetSetting<T>(this IConfiguration configuration, string settingName) =>
        configuration.GetValue<T>(settingName)
            ?? throw new InvalidOperationException($"Missing value for {settingName} setting");

    public static string GetSetting(this IConfiguration configuration, string settingName) =>
        configuration.GetSetting<string>(settingName);
}