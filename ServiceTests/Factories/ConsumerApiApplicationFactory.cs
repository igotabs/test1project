using System.Net;
using ConsumerApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ServiceTests.Factories
{
	public class ConsumerApiApplicationFactory : WebApplicationFactory<Program>
	{
		//make constructor
		public ConsumerApiApplicationFactory(HttpClient externalIdentityServerClient, HttpClient externalHelloWorldClient)
		{
			this.externalIdentityServerClient = externalIdentityServerClient;
			this.externalHelloWorldClient = externalHelloWorldClient;
		}

		public HttpClient externalIdentityServerClient { get; set; }
		public HttpClient externalHelloWorldClient { get; set; }

		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{

			builder.ConfigureTestServices(services =>
			{
				// Remove existing registrations
				services.RemoveAll(typeof(IdentityServerClient));
				services.RemoveAll(typeof(HelloWorldApiClient));

				// Register replacements with your external HttpClients
				services.AddSingleton(new IdentityServerClient(externalIdentityServerClient));
				services.AddSingleton(new HelloWorldApiClient(externalHelloWorldClient));
			});

			builder.ConfigureAppConfiguration((context, configBuilder) =>
{


	var inMemorySettingsStrings = new Dictionary<string, string?>
	{
		//[PlatformApiKafkaBootstrapServersSettingName] = BootstrapServers
	};

	configBuilder.AddInMemoryCollection(inMemorySettingsStrings);
});
		}

	}

}
