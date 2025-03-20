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
		public ConsumerApiApplicationFactory(HttpClient externalIdentityServerClient, HttpClient externalHelloWorldClient)
		{
			this._externalIdentityServerClient = externalIdentityServerClient;
			this._externalHelloWorldClient = externalHelloWorldClient;
		}

		private HttpClient _externalIdentityServerClient;
		private HttpClient _externalHelloWorldClient;

		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{

			builder.ConfigureTestServices(services =>
			{
				services.RemoveAll(typeof(IdentityServerClient));
				services.RemoveAll(typeof(HelloWorldApiClient));

				services.AddSingleton(new IdentityServerClient(_externalIdentityServerClient));
				services.AddSingleton(new HelloWorldApiClient(_externalHelloWorldClient));
			});

		}

	}

}
