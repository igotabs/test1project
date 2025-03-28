﻿using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace IdentityServerHost;

public class Program
{
	protected Program()
	{
	}

	private static void Main(string[] args)
	{
		Log.Logger = new LoggerConfiguration()
			.WriteTo.Console()
			.CreateBootstrapLogger();

		Log.Information("Starting up");

		try
		{
			var builder = WebApplication.CreateBuilder(args);

			builder.Host.UseSerilog((ctx, lc) => lc
				.MinimumLevel.Debug()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
				.MinimumLevel.Override("System", LogEventLevel.Warning)
				.MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
				.Enrich.FromLogContext()
				.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code));

			var app = builder
				.ConfigureServices()
				.ConfigurePipeline();

			app.Run();
		}
		catch (Exception ex)
		{
			Log.Fatal(ex, "Unhandled exception");
		}
		finally
		{
			Log.Information("Shut down complete");
			Log.CloseAndFlush();
		}
	}
}