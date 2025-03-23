using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using DotNet.Testcontainers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using IntegrationTest.Builders;
using IntegrationTest.Fixtures;
using IntegrationTest.Utils;
using Xunit.Abstractions;
using Environment = System.Environment;


namespace IntegrationTest.ContainerBuilders;

using IContainerConfiguration = IContainerConfiguration;

[ExcludeFromCodeCoverage]
public class ConsumerApiContainerBuilder
{
	private readonly IMessageSink _logger;
	private readonly string _network;

	public IContainer? ConsumerContainer { get; set; }

	public Dictionary<string, string> Config { get; } = new()
	{
		{ "ASPNETCORE_ENVIRONMENT", "Development" },
		{ "ASPNETCORE_URLS", "http://*:8081" },
		{ "IdentityServer__BaseUrl", "https://identityserverhost:8081" },
		{ "HelloWorldApi__BaseUrl", "http://helloworldapi:8081" },
		{ "ASPNETCORE_Kestrel__Certificates__Default__Password", "123" },
		{ "ASPNETCORE_Kestrel__Certificates__Default__Path", "/https/consumer.pfx" },
	};
	public string ConsumerApiContainerName { get; set; } = $"ConsumerApi_{Guid.NewGuid().ToString("N")}";
	public int ExposedPort { get; set; } = 5003;

	public ConsumerApiContainerBuilder(
		IMessageSink logger,
		string network)
	{
		_logger = logger;
		_network = network;
	}

	public async Task<IContainer> BuildAsync()
	{
		ConfigureConsumerContainer(ConsumerApiImageBuilder.ImageName);
		await StartConsumerContainerAsync();

		return ConsumerContainer!;
	}


	private void ConfigureConsumerContainer(string imageName)
	{
		_logger.Log($"Starting configuring Consumer container from image '{imageName}'");

		try
		{
ConsumerApiContainerName = $"ConsumerApi_{Guid.NewGuid().ToString("N")}";

	ConsumerContainer = new ContainerBuilder()
				.WithImage(imageName + ":latest")
				.WithNetwork(_network)
				.WithName(ConsumerApiContainerName)
				.WithEnvironment(Config)
				.WithPortBinding(8081, true)
				.WithCleanUp(true)
				.WithAutoRemove(true)
				.Build();
		}
		catch (Exception e)
		{
			_logger.Log("Error has occurred while configuring Consumer container");
			_logger.Log(e.Message);
			throw;
		}

		_logger.Log("Consumer container has been configured successfully");
	}

	private async Task StartConsumerContainerAsync()
	{
		try
		{
			_logger.Log($"Starting Consumer container with '{ConsumerApiContainerName}' name");

			await ConsumerContainer!.StartAsync();

		}
		catch (Exception e)
		{
			_logger.Log("Error has occured while starting Consumer container");
			_logger.Log(e.Message);

			if (ConsumerContainer?.State is TestcontainersStates.Running)
			{
				await ConsumerContainer!.DisposeAsync();
			}

			throw;
		}

		_logger.Log("Consumer container has started successfully");
	}
}
