using System.Diagnostics.CodeAnalysis;
using DotNet.Testcontainers;
using FluentAssertions;
using IntegrationTest.Builders;
using IntegrationTest.Fixtures;
using IntegrationTest.Utils;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: TestFramework($"IntegrationTest.Fixtures.{nameof(GlobalInitFixture)}", "IntegrationTest")]

namespace IntegrationTest.Fixtures;

#pragma warning disable S3881 // "IDisposable" should be implemented correctly
[ExcludeFromCodeCoverage]
public class GlobalInitFixture : XunitTestFramework, IDisposable
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
{
	private readonly IMessageSink _logger;
	public ContainersBuilder ContainersBuilder { get; set; }

	public GlobalInitFixture(IMessageSink logger) : base(logger)
    {
	    _logger = logger;
	    AssertionOptions.FormattingOptions.MaxLines = 200;
        AssertionOptions.FormattingOptions.UseLineBreaks = true;

        ConsoleLogger.Instance.DebugLogLevelEnabled = true;
        StartContainers();
    }

	private  void StartContainers()
    {
	    _logger.OnMessage(new DiagnosticMessage($"Starting all containers in Fixture {GetType().Name}"));

	    ContainersBuilder = new ContainersBuilder(_logger);


		ContainersBuilder.BuildAndStartRedisAsync().GetAwaiter().GetResult();
		ContainersBuilder.BuildAndStartIdentityServerAsync().GetAwaiter().GetResult();
		ContainersBuilder.BuildAndStartHelloWorldAsync().GetAwaiter().GetResult();
		ContainersBuilder.BuildAndStartConsumerAsync().GetAwaiter().GetResult();


		_logger.OnMessage(new DiagnosticMessage($"Finished creation of all containers in Fixture {GetType().Name}"));
    }

	public new void Dispose()
	{
		ContainersBuilder.DisposeAsync().GetAwaiter().GetResult();
	}

}
