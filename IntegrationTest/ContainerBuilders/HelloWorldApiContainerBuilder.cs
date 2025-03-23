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


namespace IntegrationTest.ContainerBuilders;


[ExcludeFromCodeCoverage]
public class HelloWorldApiContainerBuilder
{
	private readonly IMessageSink _logger;
	private readonly string _network;

	private IContainer? HelloWorldContainer { get; set; }

	public Dictionary<string, string> Config { get; } = new()
	{
		{ "ASPNETCORE_ENVIRONMENT", "Development" },
		{"ASPNETCORE_URLS", "http://*:8081"},
		{ "IdentityServer__BaseUrl", "https://identityserverhost:8081" },
		{ "HelloWorldApi__BaseUrl", "http://helloworldapi:8081" },
		{ "Redis__Host", "redis" },
		{ "ASPNETCORE_Kestrel__Certificates__Default__Password", "123" },
		{ "ASPNETCORE_Kestrel__Certificates__Default__Path", "/https/helloworld.pfx" },
	};
	public string HelloWorldApiContainerName { get; set; } = $"helloworldapi_{DateTime.Now:HHmmssfff}";
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
		ConfigureHelloWorldContainer(HelloWorldApiImageBuilder.ImageName);
		await StartHelloWorldContainerAsync();

		return HelloWorldContainer!;
	}


	private void ConfigureHelloWorldContainer(string imageName)
	{
		_logger.Log($"Starting configuring Hello World container from image '{imageName}'");

		try
		{
			HelloWorldContainer = new ContainerBuilder()
				.WithImage(imageName + ":latest")
				.WithNetwork(_network)
				.WithCreateParameterModifier(param => param.User = "root")
				.WithName(HelloWorldApiContainerName)
				.WithNetworkAliases(new[] { "helloworldapi" })
				.WithEnvironment(Config)
				.WithPortBinding(ExposedPort, 8081)
				.WithCleanUp(true)
				.WithAutoRemove(true)
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

	private async Task StartHelloWorldContainerAsync()
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
