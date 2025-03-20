using System.Diagnostics.CodeAnalysis;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace IntegrationTest.Utils;

[ExcludeFromCodeCoverage]
public static class IMessageSinkExtensions
{
    public static bool Log(this IMessageSink messageSink, string message)
    {
        return messageSink.Log("[{0}] {1}", DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss:ffff"), message);
    }

    public static bool Log(this IMessageSink messageSink, string message, params object[] args)
    {
        return messageSink.OnMessage(new DiagnosticMessage(message, args));
    }
}
