#nullable enable
using System.Diagnostics.CodeAnalysis;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Networks;
using IntegrationTest.ContainerBuilders;
using IntegrationTest.Utils;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using Xunit.Sdk;
using IContainer = DotNet.Testcontainers.Containers.IContainer;

namespace IntegrationTest.Builders
{
    [ExcludeFromCodeCoverage]
    public class ContainersBuilder : IAsyncDisposable
    {
	    private string _network; 

        private readonly List<IContainer> _startedContainers = [];

		private readonly RedisContainerBuilder _redisContainerBuilder;
		private readonly IdentityServerContainerBuilder _identityServerContainerBuilder;
		private readonly HelloWorldApiContainerBuilder _helloWorldApiContainerBuilder;
		private readonly ConsumerApiContainerBuilder _consumerApiContainerBuilder;


		private readonly IdentityServerHostImageBuilder _identityServerImageBuilder;
		private readonly HelloWorldApiImageBuilder _helloWorldApiImageBuilder;
		private readonly ConsumerApiImageBuilder _consumerApiImageBuilder;

        public ContainersBuilder(IMessageSink messageSink)
        {
			_network = NetworkResolverUtils.GetNetwork();

			_identityServerImageBuilder = new IdentityServerHostImageBuilder(messageSink);
			_helloWorldApiImageBuilder = new HelloWorldApiImageBuilder(messageSink);
			_consumerApiImageBuilder = new ConsumerApiImageBuilder(messageSink);


			_redisContainerBuilder = new RedisContainerBuilder(messageSink, _network);
			_identityServerContainerBuilder = new IdentityServerContainerBuilder(messageSink, _network);
			_helloWorldApiContainerBuilder = new HelloWorldApiContainerBuilder(messageSink, _network);
			_consumerApiContainerBuilder = new ConsumerApiContainerBuilder(messageSink, _network);
		}

        public async Task BuildAndStartRedisAsync()
        {
	        _startedContainers.Add(
			await _redisContainerBuilder.BuildAsync());
        }

		public async Task BuildAndStartIdentityServerAsync()
        {
	        await _identityServerImageBuilder.CreateIdentityServerHostFromDockerFileAsync();
	        _startedContainers.Add(
			await _identityServerContainerBuilder.BuildAsync());
        }


		public async Task BuildAndStartHelloWorldAsync()
        {
            await _helloWorldApiImageBuilder.CreateHelloWorldApiFromDockerFileAsync();
            _startedContainers.Add(
			await _helloWorldApiContainerBuilder.BuildAsync());
        }

        public async Task BuildAndStartConsumerAsync()
        {
            await _consumerApiImageBuilder.CreateConsumerApiFromDockerFileAsync();
            _startedContainers.Add(
			await _consumerApiContainerBuilder.BuildAsync());
        }

        public async ValueTask DisposeAsync()
        {
	        foreach (var container in _startedContainers)
	        {
		        await container.DisposeAsync();
	        }

	        //if (_identityServerImageBuilder.DockerImage != null)
	        //{
		       // await _identityServerImageBuilder.DockerImage.DeleteAsync();
		       // await _identityServerImageBuilder.DockerImage.DisposeAsync().AsTask();
	        //}


	        //if (_helloWorldApiImageBuilder.DockerImage != null)
	        //{
		       // await _helloWorldApiImageBuilder.DockerImage.DeleteAsync();
		       // await _helloWorldApiImageBuilder.DockerImage.DisposeAsync().AsTask();
	        //}


	        //if (_consumerApiImageBuilder.DockerImage != null)
	        //{
		       // await _consumerApiImageBuilder.DockerImage.DeleteAsync();
		       // await _consumerApiImageBuilder.DockerImage.DisposeAsync().AsTask();
	        //}

	        GC.SuppressFinalize(this);
        }
    }
}
