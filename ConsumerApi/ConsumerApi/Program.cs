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