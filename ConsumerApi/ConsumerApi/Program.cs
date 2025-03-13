
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo() { Title = "Consumer.API", Version = "v1" });
});

builder.Services.AddHttpClient("HelloWorldClient", client =>
	{
		client.BaseAddress = new Uri(builder.Configuration["HelloWorldApi:BaseUrl"] ?? throw new ArgumentException("HelloWorldApi not found"));
	})
	.ConfigurePrimaryHttpMessageHandler(() =>
	{
		return new HttpClientHandler
		{
			ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
		};
	});

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
