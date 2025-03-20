using System.Net;
using HelloWorldApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace ServiceTests.Factories
{
    public class HelloWorldApiApiApplicationFactory() : WebApplicationFactory<Program>
    {
		private HttpClient externalIdentityServerClient;

		public HelloWorldApiApiApplicationFactory(HttpClient externalIdentityServerClient) : this()
		{
		    this.externalIdentityServerClient = externalIdentityServerClient;
	    }
		protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
			builder.ConfigureTestServices(services =>
			{
				services.Configure<JwtBearerOptions>("token", options =>
				{
					// Replace the default Backchannel with your custom HttpClient.
					options.Backchannel = externalIdentityServerClient;
				});
			});

        }

    }

}
