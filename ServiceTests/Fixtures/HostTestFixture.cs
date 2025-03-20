using Microsoft.AspNetCore.Mvc.Testing;
using ServiceTests.Factories;
using ServiceTests.Utils;
using System.Net.Http;

namespace ServiceTests.Fixtures;

public class HostTestFixture : IAsyncLifetime
{
	private RedisContainerManager _redisContainerManager;

	public HostTestFixture()
	{
	}

	public async Task InitializeAsync()
	{
		Environment.SetEnvironmentVariable("Istest", "true");

		_redisContainerManager = await RedisContainerManager.CreateAndStartRedisContainerAsync();

		RedisAddress = $"{_redisContainerManager.Hostname}:{_redisContainerManager.Port}";

		IdentityServerApplicationFactory = new IdentityServerApplicationFactory();
		var options = new WebApplicationFactoryClientOptions
		{
			BaseAddress = new Uri("https://localhost")
		};
		IdentityClient = IdentityServerApplicationFactory.CreateClient();

		HelloWorldApiApiApplicationFactory = new HelloWorldApiApiApplicationFactory(IdentityClient);
		HelloWorldClient = HelloWorldApiApiApplicationFactory.CreateClient();

		ConsumerApiApplicationFactory = new ConsumerApiApplicationFactory(IdentityClient, HelloWorldClient);

		ConsumerApiApplicationFactory.externalHelloWorldClient = HelloWorldClient;
		ConsumerApiApplicationFactory.externalHelloWorldClient = HelloWorldClient;
		ConsumerClient = ConsumerApiApplicationFactory.CreateClient();

		//httpClient.BaseAddress = new Uri("http://localhost:5003/");
		//warm-up
		//await Task.Delay(20000);
		//var response = await ConsumerClient.GetStringAsync($"ConsumeHelloWorld?count=1");

	}

	public HttpClient IdentityClient { get; set; }

	public HttpClient HelloWorldClient { get; set; }

	public HttpClient ConsumerClient { get; set; }

	public string RedisAddress { get; set; }
	public IdentityServerApplicationFactory IdentityServerApplicationFactory { get; private set; }
	public HelloWorldApiApiApplicationFactory HelloWorldApiApiApplicationFactory { get; private set; }
	public ConsumerApiApplicationFactory ConsumerApiApplicationFactory { get; private set; }

	public async Task DisposeAsync()
	{
		await IdentityServerApplicationFactory.DisposeAsync();
		await HelloWorldApiApiApplicationFactory.DisposeAsync();
		await ConsumerApiApplicationFactory.DisposeAsync();

	}
} 