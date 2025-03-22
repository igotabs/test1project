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
public class IdentityServerContainerBuilder
{
	private readonly IMessageSink _logger;
	private readonly string _network;

	private IContainer? IdentityServerContainer { get; set; }

	public Dictionary<string, string> Config { get; } = new()
	{
		{ "ASPNETCORE_ENVIRONMENT", "Development" },
		{ "ASPNETCORE_HTTP_PORTS", "8080" },
		{ "ASPNETCORE_HTTPS_PORTS", "8081" },
		{ "IdentityServer__BaseUrl", "https://identityserverhost:8081" },
		{ "HelloWorldApi__BaseUrl", "http://helloworldapi:8081" },
	};
	public string IdentityServerContainerName { get; set; } = $"identityserver_{DateTime.Now:HHmmssfff}";
	public int ExposedPort { get; set; } = 5001;

	public IdentityServerContainerBuilder(
		IMessageSink logger,
		string network)
	{
		_logger = logger;
		_network = network;
	}

	public async Task<IContainer> BuildAsync()
	{
		ConfigureIdentityServerContainer(IdentityServerHostImageBuilder.ImageName);
		await StartIdentityServerContainerAsync();

		return IdentityServerContainer!;
	}


	private void ConfigureIdentityServerContainer(string imageName)
	{
		_logger.Log($"Starting configuring Idenyty Server container from image '{imageName}'");
		var appData = Environment.GetEnvironmentVariable("APPDATA");

		try
		{
			IdentityServerContainer = new ContainerBuilder()
				.WithImage(imageName + ":latest")
				.WithNetwork(_network)
				//.WithCreateParameterModifier(param => param.User = "root")
				.WithName(IdentityServerContainerName)
				.WithNetworkAliases(new[] { "identityserverhost" })
				.WithEnvironment(Config)
				//.WithHostname(Environment.GetEnvironmentVariable("COMPUTERNAME"))
				.WithPortBinding(56205, 8080)
				.WithPortBinding(ExposedPort,8081)
				.WithBindMount(
					Path.Combine(appData, "Microsoft", "UserSecrets"),
					"/home/app/.microsoft/usersecrets",
					AccessMode.ReadOnly)
				.WithBindMount(
					Path.Combine(appData, "ASP.NET", "Https"),
					"/home/app/.aspnet/https",
					AccessMode.ReadOnly)
				.WithCleanUp(true)
				.WithAutoRemove(true)
				//.WithLogger(ConsoleLogger.Instance)
				.Build();
		}
		catch (Exception e)
		{
			_logger.Log("Error has occurred while configuring Idenyty Server container");
			_logger.Log(e.Message);
			throw;
		}

		_logger.Log("Idenyty Server container has been configured successfully");
	}

	private async Task StartIdentityServerContainerAsync()
	{
		try
		{
			_logger.Log($"Starting Idenyty Server container with '{IdentityServerContainerName}' name");

			await IdentityServerContainer!.StartAsync();

		}
		catch (Exception e)
		{
			_logger.Log("Error has occured while starting Idenyty Server container");
			_logger.Log(e.Message);

			if (IdentityServerContainer?.State is TestcontainersStates.Running)
			{
				await IdentityServerContainer!.DisposeAsync();
			}

			throw;
		}

		_logger.Log("Idenyty Server container has started successfully");
	}
}
