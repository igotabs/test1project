using StackExchange.Redis;
const string RedisHost = "Redis:Host";
const string RedisPort = "Redis:Port";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
