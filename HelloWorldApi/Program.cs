using HelloWorldApi.Tools;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using StackExchange.Redis;

namespace HelloWorldApi;

public class Program
{
	const string RedisHost = "Redis:Host";
	const string RedisPort = "Redis:Port";
	protected Program()
	{
	}

	private static void Main(string[] args)
	{
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
		var s= builder.Configuration["IdentityServer:BaseUrl"];
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
		bool isTest = false;
		var testEnv = Environment.GetEnvironmentVariable("Istest");
		if (!string.IsNullOrEmpty(testEnv))
		{
			bool.TryParse(testEnv, out isTest);
		}

		string identityServerAddress = !isTest
			? builder.Configuration["IdentityServer:BaseUrl"]
			: "http://localhost";
		var i = 1;
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

				options.Events = new JwtBearerEvents
				{
					OnMessageReceived = context =>
					{
						// Log or inspect the incoming token here
						Console.WriteLine("OnMessageReceived invoked.");
						return Task.CompletedTask;
					},
					OnTokenValidated = context =>
					{
						// Log or inspect the validated token details
						Console.WriteLine("OnTokenValidated invoked.");
						return Task.CompletedTask;
					},
					OnAuthenticationFailed = context =>
					{
						// Log the error details if authentication fails
						Console.WriteLine("OnAuthenticationFailed invoked: " + context.Exception.Message);
						return Task.CompletedTask;
					},
					OnChallenge = context =>
					{
						// Log the challenge response details
						Console.WriteLine("OnChallenge invoked.");
						return Task.CompletedTask;
					}
				};
			});

		var app = builder.Build();

		app.UseMiddleware<RequestInterceptorMiddleware>();

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
	}
}