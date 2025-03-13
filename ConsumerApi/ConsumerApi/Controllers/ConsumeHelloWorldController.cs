using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ConsumerApi.TokenService;
using ConsumerApi.Tools;
using Duende.IdentityModel;
using Duende.IdentityModel.Client;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace ConsumerApi.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class ConsumeHelloWorldController : ControllerBase
	{
		private static readonly string[] Summaries = new[]
		{
			"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
		};

		private readonly string? _identityServerBaseUrl;
		private readonly IHelloWorldTokenService _tokenService;
		private readonly ILogger<ConsumeHelloWorldController> _logger;
		private readonly string? _helloWorldApiBaseUrl;

		public ConsumeHelloWorldController(
			IConfiguration configuration,
			IHelloWorldTokenService tokenService,
			ILogger<ConsumeHelloWorldController> logger)
		{
			_identityServerBaseUrl = configuration["IdentityServer:BaseUrl"] ?? throw new ArgumentNullException(nameof(_identityServerBaseUrl));
			_helloWorldApiBaseUrl = configuration["HelloWorldApi:BaseUrl"] ?? throw new ArgumentNullException(nameof(_helloWorldApiBaseUrl));
			_tokenService = tokenService;
			_logger = logger;
		}

		[HttpGet(Name = "GetWeatherForecast")]
		public async Task<IEnumerable<HelloWorld?>> Get([FromQuery] int count = 1)
		{
			var token = await _tokenService.GetAccessTokenAsync();

			// 2. Make multiple requests in parallel
			var results = new ConcurrentBag<HelloWorld>();

			await Parallel.ForEachAsync(
				Enumerable.Range(1, count),
				async (index, cancellationToken) =>
				{
					var item = await CallServiceAsync(token);
					if (item != null) results.Add(item);
				}
			);

			// 3. Return the accumulated list
			return results.ToList();
		}
		async Task<HelloWorld?> CallServiceAsync(string token)
		{
			var httpClientHandler = new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
				{
					// Return 'true' to allow any cert
					return true;
				}
			};
			var httpClient = new HttpClient(httpClientHandler);
			httpClient.BaseAddress = new Uri(_helloWorldApiBaseUrl);
			httpClient.SetBearerToken(token);
			var response = await httpClient.GetStringAsync($"HelloWorld");

			"\n\nService claims:".ConsoleGreen();
			Console.WriteLine(response.PrettyPrintJson());
			var result = JsonConvert.DeserializeObject< HelloWorld>(response);

			return result;
		}
	}
}
