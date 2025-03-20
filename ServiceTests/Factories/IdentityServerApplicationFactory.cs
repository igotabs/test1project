using System.Net;
using IdentityServerHost;
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
    public class IdentityServerApplicationFactory() : WebApplicationFactory<Program>
    {
		protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
	        //builder.ConfigureKestrel(serverOptions =>
	        //{
		       // serverOptions.Listen(IPAddress.Loopback, 5001, listenOptions =>
		       // {
			      //  listenOptions.UseHttps(); // This enables HTTPS on port 5001.
		       // });
	        //});

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
