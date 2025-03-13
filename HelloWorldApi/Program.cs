using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using RedLockNet.SERedis.Configuration;
using RedLockNet.SERedis;
using RedLockNet;
using StackExchange.Redis;
using Microsoft.OpenApi.Models;

const string RedisHost = "Redis:Host";
const string RedisPort = "Redis:Port";


Console.Title = "Simple API";
Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
                .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo() { Title = "HelloWorld.API", Version = "v1" });
});

builder.Services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
{
	var host = builder.Configuration.GetSetting(RedisHost);
	var port = builder.Configuration.GetSetting<ushort>(RedisPort);

	return ConnectionMultiplexer.Connect(new ConfigurationOptions
	{
		EndPoints =
		{
			{ host, port }
		}
	});
});

builder.Services.AddSingleton<IDistributedLockFactory>(serviceProvider =>
{
	var connectionMultiplexer = serviceProvider
		.GetRequiredService<IConnectionMultiplexer>() as ConnectionMultiplexer;
	List<RedLockMultiplexer> redLockMultiplexers = [connectionMultiplexer];
	var redLockFactory = RedLockFactory.Create(redLockMultiplexers);
	return redLockFactory;
});

builder.Services.AddSerilog();

builder.Services.AddControllers();

// this API will accept any access token from the authority
string identityServerAddress = builder.Configuration["IdentityServer:BaseUrl"];
builder.Services.AddAuthentication("token")
    .AddJwtBearer("token", options =>
    {
        options.Authority = identityServerAddress;

        options.RequireHttpsMetadata = false;
        options.BackchannelHttpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        options.TokenValidationParameters.ValidateAudience = false;

        options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
        options.MapInboundClaims = false;
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireAuthorization();

app.Run();