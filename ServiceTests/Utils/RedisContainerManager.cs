using Testcontainers.Redis;

namespace ServiceTests.Utils;

public class RedisContainerManager
{
    private readonly RedisContainer _redisContainer;
    public readonly string Hostname;
    public ushort Port;

    // Private constructor to prevent instantiation outside the class
    private RedisContainerManager(RedisContainer redisContainer, string hostname, ushort port)
    {
        _redisContainer = redisContainer;
        Hostname = hostname;
        Port = port;
    }

    // Static factory method to create and start the Redis container
    public static async Task<RedisContainerManager> CreateAndStartRedisContainerAsync()
    {
        var redisContainer = new RedisBuilder()
            .WithImage("redis/redis-stack-server:latest")
            .WithPortBinding(6379, 6379)
            .Build();

        await redisContainer.StartAsync();

        var hostName = redisContainer.Hostname;
        var port = redisContainer.GetMappedPublicPort(6379);

        return new RedisContainerManager(redisContainer, hostName, port);
    }

    // Method to stop the Kafka container
    public async Task StopRedisContainerAsync()
    {
        if (_redisContainer != null)
        {
            await _redisContainer.StopAsync();
            await _redisContainer.DisposeAsync();
        }
    }
}
