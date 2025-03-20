using ServiceTests.Factories;
using ServiceTests.Utils;

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
		IdentityServerApplicationFactory.CreateClient();

		HelloWorldApiApiApplicationFactory = new HelloWorldApiApiApplicationFactory();
		HelloWorldApiApiApplicationFactory.CreateClient();

		ConsumerApiApplicationFactory = new ConsumerApiApplicationFactory();
		ConsumerClient = ConsumerApiApplicationFactory.CreateClient();

	}

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