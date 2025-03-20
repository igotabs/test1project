using ConsumerApi.HttpClients;
using ConsumerApi.TokenService;
using Microsoft.OpenApi.Models;

namespace ConsumerApi;

public class Program
{
	protected Program()
	{
	}

	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		builder.Services.AddControllers();
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen(c =>
		{
			c.SwaggerDoc("v1", new OpenApiInfo() { Title = "Consumer.API", Version = "v1" });
		});

		builder.Services.AddMemoryCache();

		builder.Services.AddSingleton<IdentityServerClient>(sp =>
		{
			var configuration = sp.GetRequiredService<IConfiguration>();
			var baseUrl = configuration["IdentityServer:BaseUrl"];
			var handler = new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
			};

			var client = new HttpClient(handler)
			{
				BaseAddress = new Uri(baseUrl)
			};

			return new IdentityServerClient(client);
		});

		builder.Services.AddSingleton<HelloWorldApiClient>(sp =>
		{
			var configuration = sp.GetRequiredService<IConfiguration>();
			var baseUrl = configuration["HelloWorldApi:BaseUrl"];
			var handler = new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
			};

			var client = new HttpClient(handler)
			{
				BaseAddress = new Uri(baseUrl)
			};

			return new HelloWorldApiClient(client);
		});

		builder.Services.AddSingleton<IHelloWorldTokenService, HelloWorldTokenService>();

		var app = builder.Build();

// Configure the HTTP request pipeline.
		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.UseHttpsRedirection();

		app.UseAuthorization();

		app.MapControllers();

		app.Run();
	}
}