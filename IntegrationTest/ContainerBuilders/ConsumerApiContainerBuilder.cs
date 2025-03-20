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
public class ConsumerApiContainerBuilder //: IContainerBuilder, IOpcUaServerContainerConfiguration
{
	private readonly IMessageSink _logger;
	private readonly string _network;

	private IContainer? ConsumerContainer { get; set; }

	public Dictionary<string, string> Config { get; } = new()
	{
		{ "ASPNETCORE_ENVIRONMENT", "Development" },
		{"ASPNETCORE_URLS", "http://*:8081"},
		//{ "ASPNETCORE_Kestrel:Certificates:Default:Password", "Development" },

		{ "IdentityServer__BaseUrl", "https://identityserverhost:8081" },
		{ "HelloWorldApi__BaseUrl", "http://helloworldapi:8081" },
	};
	public string ConsumerApiContainerName { get; set; } = "consumerapi";
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
		ConfigureOpcUaServerContainer(ConsumerApiImageBuilder.ImageName);
		await StartOpcUaServerContainerAsync();

		return ConsumerContainer!;
	}


	private void ConfigureOpcUaServerContainer(string imageName)
	{
		_logger.Log($"Starting configuring Consumer container from image '{imageName}'");

		try
		{
			ConsumerContainer = new ContainerBuilder()
				.WithImage(imageName + ":latest")
				.WithNetwork(_network)
				//.WithCreateParameterModifier(param => param.User = "root")
				.WithName(ConsumerApiContainerName)
				.WithEnvironment(Config)
				//.WithHostname(Environment.GetEnvironmentVariable("COMPUTERNAME"))
				//.WithPortBinding(8080, true)
				.WithPortBinding(ExposedPort, 8081)
				.WithCleanUp(true)
				.WithAutoRemove(true)
				//.WithLogger(ConsoleLogger.Instance)
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

	private async Task StartOpcUaServerContainerAsync()
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
