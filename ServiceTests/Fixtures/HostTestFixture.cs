using Microsoft.AspNetCore.Mvc.Testing;
using ServiceTests.Factories;
using ServiceTests.Utils;
using System.Net.Http;

namespace ServiceTests.Fixtures;

public class HostTestFixture : IAsyncLifetime
{
	public required RedisContainerManager RedisContainerManager { get; set; }
	public required HttpClient IdentityClient { get; set; }

	public required HttpClient HelloWorldClient { get; set; }

	public required HttpClient ConsumerClient { get; set; }

	public required string RedisAddress { get; set; }
	public IdentityServerApplicationFactory? IdentityServerApplicationFactory { get; private set; }
	public HelloWorldApiApiApplicationFactory? HelloWorldApiApiApplicationFactory { get; private set; }
	public ConsumerApiApplicationFactory? ConsumerApiApplicationFactory { get; private set; }


	public async Task InitializeAsync()
	{
		Environment.SetEnvironmentVariable("IsTest", "true");

		RedisContainerManager = await RedisContainerManager.CreateAndStartRedisContainerAsync();

		RedisAddress = $"{RedisContainerManager.Hostname}:{RedisContainerManager.Port}";

		IdentityServerApplicationFactory = new IdentityServerApplicationFactory();
		var options = new WebApplicationFactoryClientOptions
		{
			BaseAddress = new Uri("https://localhost")
		};
		IdentityClient = IdentityServerApplicationFactory.CreateClient();

		HelloWorldApiApiApplicationFactory = new HelloWorldApiApiApplicationFactory(IdentityClient);
		HelloWorldClient = HelloWorldApiApiApplicationFactory.CreateClient();

		ConsumerApiApplicationFactory = new ConsumerApiApplicationFactory(IdentityClient, HelloWorldClient);

		ConsumerClient = ConsumerApiApplicationFactory.CreateClient();

	}

	public async Task DisposeAsync()
	{
		if (IdentityServerApplicationFactory != null)
		{
			await IdentityServerApplicationFactory.DisposeAsync();
		}
		if (HelloWorldApiApiApplicationFactory != null)
		{
			await HelloWorldApiApiApplicationFactory.DisposeAsync();
		}
		if (ConsumerApiApplicationFactory != null)
		{
			await ConsumerApiApplicationFactory.DisposeAsync();
		}
	}
} 