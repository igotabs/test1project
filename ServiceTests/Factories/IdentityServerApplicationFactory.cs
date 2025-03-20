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

			builder.ConfigureAppConfiguration((context, configBuilder) =>
            {

            });
        }

    }

}
