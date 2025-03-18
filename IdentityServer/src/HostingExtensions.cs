using IdentityServerHost.Misc;
using Microsoft.OpenApi.Models;
using Serilog;

namespace IdentityServerHost;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo() { Title = "IdentityServer.API", Version = "v1" });
        });
        var idsvrBuilder = builder.Services.AddIdentityServer(options =>
            {
                options.KeyManagement.Enabled = false;
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/api_scopes
                options.EmitStaticAudienceClaim = true;
                options.PushedAuthorization.AllowUnregisteredPushedRedirectUris = true;
            })
            .AddTestUsers(TestUsers.Users);

        idsvrBuilder.AddInMemoryIdentityResources(Resources.Identity);
        idsvrBuilder.AddInMemoryApiScopes(Resources.ApiScopes);
        idsvrBuilder.AddInMemoryApiResources(Resources.ApiResources);
        idsvrBuilder.AddInMemoryClients(Clients.List);
        idsvrBuilder.AddDeveloperSigningCredential();

        // this is only needed for the JAR and JWT samples and adds supports for JWT-based client authentication
        idsvrBuilder.AddJwtBearerClientAuthentication();

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseIdentityServer();

        return app;
    }
}