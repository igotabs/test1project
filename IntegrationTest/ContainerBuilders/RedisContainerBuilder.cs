﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using DotNet.Testcontainers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using IntegrationTest.Builders;
using IntegrationTest.Fixtures;
using IntegrationTest.Utils;
using Testcontainers.Redis;
using Xunit.Abstractions;
using Environment = System.Environment;


namespace IntegrationTest.ContainerBuilders;

using IContainerConfiguration = IContainerConfiguration;

[ExcludeFromCodeCoverage]
public class RedisContainerBuilder
{
	private readonly IMessageSink _logger;
	private readonly string _network;

	private IContainer? RedisContainer { get; set; }

	public string RedisContainerName { get; set; } = $"redis_{Guid.NewGuid().ToString("N")}";
	public int ExposedPort { get; set; } = 6379;

	public RedisContainerBuilder(
		IMessageSink logger,
		string network)
	{
		_logger = logger;
		_network = network;
	}

	public async Task<IContainer> BuildAsync()
	{
		ConfigureRedisContainer(HelloWorldApiImageBuilder.ImageName);
		await StartRedisContainerAsync();

		return RedisContainer!;
	}


	private void ConfigureRedisContainer(string imageName)
	{
		_logger.Log($"Starting configuring Redis container from image '{imageName}'");

		try
		{
			RedisContainer = new RedisBuilder()
				.WithImage("redis/redis-stack-server:latest")
				.WithName(RedisContainerName)
				.WithNetworkAliases(new[] { "redis" })
				.WithNetwork(_network)
				.WithPortBinding(6379, ExposedPort)
				.Build();
		}
		catch (Exception e)
		{
			_logger.Log("Error has occurred while configuring Redis container");
			_logger.Log(e.Message);
			throw;
		}

		_logger.Log("Redis container has been configured successfully");
	}

	private async Task StartRedisContainerAsync()
	{
		try
		{
			_logger.Log($"Starting Redis container with name");

			await RedisContainer!.StartAsync();

		}
		catch (Exception e)
		{
			_logger.Log("Error has occured while starting Redis container");
			_logger.Log(e.Message);

			if (RedisContainer?.State is TestcontainersStates.Running)
			{
				await RedisContainer!.DisposeAsync();
			}

			throw;
		}

		_logger.Log("Redis container has started successfully");
	}
}
