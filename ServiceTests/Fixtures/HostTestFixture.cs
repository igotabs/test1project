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
		_redisContainerManager = await RedisContainerManager.CreateAndStartRedisContainerAsync();

		RedisAddress = $"{_redisContainerManager.Hostname}:{_redisContainerManager.Port}";

		IdentityServerApplicationFactory = new IdentityServerApplicationFactory();
		ConsumerClient3 = IdentityServerApplicationFactory.CreateClient();

		HelloWorldApiApiApplicationFactory = new HelloWorldApiApiApplicationFactory();
		ConsumerClient2 = HelloWorldApiApiApplicationFactory.CreateClient();

		ConsumerApiApplicationFactory = new ConsumerApiApplicationFactory();
		ConsumerClient = ConsumerApiApplicationFactory.CreateClient();

		//httpClient.BaseAddress = new Uri("http://localhost:5003/");
		//warm-up
		//await Task.Delay(20000);
		var response = await ConsumerClient.GetStringAsync($"ConsumeHelloWorld?count=1");

	}

	public HttpClient ConsumerClient3 { get; set; }

	public HttpClient ConsumerClient2 { get; set; }

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