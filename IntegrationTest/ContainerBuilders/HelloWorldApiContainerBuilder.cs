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
public class HelloWorldApiContainerBuilder //: IContainerBuilder, IOpcUaServerContainerConfiguration
{
	private readonly IMessageSink _logger;
	private readonly string _network;

	private IContainer? HelloWorldContainer { get; set; }

	public Dictionary<string, string> Config { get; } = new()
	{
		{ "ASPNETCORE_ENVIRONMENT", "Development" },
		{ "ASPNETCORE_URLS", "https://*:8081" },
		{ "ASPNETCORE_Kestrel:Certificates:Default:Password", "Development" },
		{ "IdentityServer__BaseUrl", "https://identityserverhost:8081" },
		{ "HelloWorldApi__BaseUrl", "https://helloworldapi:8081" },
	};
	public string HelloWorldApiContainerName { get; set; } = "helloworldapi";
	public int ExposedPort { get; set; } = 5002;

	public HelloWorldApiContainerBuilder(
		IMessageSink logger,
		string network)
	{
		_logger = logger;
		_network = network;
	}

	public async Task<IContainer> BuildAsync()
	{
		ConfigureOpcUaServerContainer(HelloWorldApiImageBuilder.ImageName);
		await StartOpcUaServerContainerAsync();

		return HelloWorldContainer!;
	}


	private void ConfigureOpcUaServerContainer(string imageName)
	{
		_logger.Log($"Starting configuring Hello World container from image '{imageName}'");

		try
		{
			HelloWorldContainer = new ContainerBuilder()
				.WithImage(imageName + ":latest")
				.WithNetwork(_network)
				.WithCreateParameterModifier(param => param.User = "root")
				.WithName(HelloWorldApiContainerName)
				.WithEnvironment(Config)
				.WithHostname(Environment.GetEnvironmentVariable("COMPUTERNAME"))
				.WithPortBinding(8080, true)
				.WithPortBinding(8081, ExposedPort)
				.WithCleanUp(true)
				.WithAutoRemove(true)
				.WithLogger(ConsoleLogger.Instance)
				.Build();
		}
		catch (Exception e)
		{
			_logger.Log("Error has occurred while configuring Hello World container");
			_logger.Log(e.Message);
			throw;
		}

		_logger.Log("Hello World container has been configured successfully");
	}

	private async Task StartOpcUaServerContainerAsync()
	{
		try
		{
			_logger.Log($"Starting Hello World container with '{HelloWorldApiContainerName}' name");

			await HelloWorldContainer!.StartAsync();

		}
		catch (Exception e)
		{
			_logger.Log("Error has occured while starting Hello World container");
			_logger.Log(e.Message);

			if (HelloWorldContainer?.State is TestcontainersStates.Running)
			{
				await HelloWorldContainer!.DisposeAsync();
			}

			throw;
		}

		_logger.Log("Hello World container has started successfully");
	}
}
