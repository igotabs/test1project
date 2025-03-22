using Microsoft.AspNetCore.Mvc.Testing;
using ServiceTests.Factories;
using ServiceTests.Utils;
using System.Collections.Concurrent;
using System.Net.Http;

namespace ServiceTests.Fixtures;

public class HostTestFixture : IAsyncLifetime
{
	private readonly ConcurrentBag<ConsumerApiApplicationFactory> _consumerFactories = new ();

	public required RedisContainerManager RedisContainerManager { get; set; }
	public required HttpClient IdentityClient { get; set; }

	public required HttpClient HelloWorldClient { get; set; }
	public required List<HttpClient> ConsumerClients { get; set; }
	public required string RedisAddress { get; set; }
	public IdentityServerApplicationFactory? IdentityServerApplicationFactory { get; private set; }
	public HelloWorldApiApiApplicationFactory? HelloWorldApiApiApplicationFactory { get; private set; }

	public async Task InitializeAsync()
	{
		Environment.SetEnvironmentVariable("IsTest", "true");

		RedisContainerManager = await RedisContainerManager.CreateAndStartRedisContainerAsync();

		RedisAddress = $"{RedisContainerManager.Hostname}:{RedisContainerManager.Port}";

		IdentityServerApplicationFactory = new IdentityServerApplicationFactory();
		IdentityClient = IdentityServerApplicationFactory.CreateClient();
		await Task.Delay(1000);
		HelloWorldApiApiApplicationFactory = new HelloWorldApiApiApplicationFactory(IdentityClient);
		HelloWorldClient = HelloWorldApiApiApplicationFactory.CreateClient();
		
		ConsumerClients = await CreateClientsFromDifferentFactoriesAsync(1);

	}

	public async Task<List<HttpClient>> CreateClientsFromDifferentFactoriesAsync(int count)
	{
		var tasks = new List<Task<HttpClient>>();

		for (int i = 0; i < count; i++)
		{
			tasks.Add(Task.Run(() =>
			{
				var factory = new ConsumerApiApplicationFactory(IdentityClient, HelloWorldClient);
				_consumerFactories.Add(factory);
				return factory.CreateClient();
			}));
		}

		return (await Task.WhenAll(tasks)).ToList();
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

		foreach (var factory in _consumerFactories)
		{
			await factory.DisposeAsync();
		}
		await RedisContainerManager.StopRedisContainerAsync();
	}
} 