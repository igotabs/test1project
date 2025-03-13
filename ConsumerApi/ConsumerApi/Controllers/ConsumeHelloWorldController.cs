using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ConsumerApi.Tools;
using Duende.IdentityModel;
using Duende.IdentityModel.Client;
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
		private readonly ILogger<ConsumeHelloWorldController> _logger;
		private readonly string? _helloWorldApiBaseUrl;

		public ConsumeHelloWorldController(
			IConfiguration configuration,
			ILogger<ConsumeHelloWorldController> logger)
		{
			_identityServerBaseUrl = configuration["IdentityServer:BaseUrl"] ?? throw new ArgumentNullException(nameof(_identityServerBaseUrl));
			_helloWorldApiBaseUrl = configuration["HelloWorldApi:BaseUrl"] ?? throw new ArgumentNullException(nameof(_helloWorldApiBaseUrl));
			_logger = logger;
		}

		[HttpGet(Name = "GetWeatherForecast")]
		public async Task<IEnumerable<HelloWorld?>> Get([FromQuery] int count = 1)
		{
			var jwk = new JsonWebKey(Constants.RsaKey);
			var response = await RequestTokenAsync(new SigningCredentials(jwk, "RS256"));
			response.Show();
			if (String.IsNullOrEmpty(response.AccessToken)) 
				return new List<HelloWorld?>();

			// 2. Make multiple requests in parallel
			var results = new ConcurrentBag<HelloWorld>();

			await Parallel.ForEachAsync(
				Enumerable.Range(1, count),
				async (index, cancellationToken) =>
				{
					var item = await CallServiceAsync(response.AccessToken);
					if (item != null) results.Add(item);
				}
			);

			// 3. Return the accumulated list
			return results.ToList();
		}


		async Task<TokenResponse> RequestTokenAsync(SigningCredentials signingCredentials)
		{
			var httpClientHandler = new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
				{
					// Return 'true' to allow any cert
					return true;
				}
			};
			var client = new HttpClient(httpClientHandler);

			var disco = await client.GetDiscoveryDocumentAsync(_identityServerBaseUrl);
			if (disco.IsError) throw new Exception(disco.Error);

			var clientToken = CreateClientToken(signingCredentials, "jwt.client.credentials.sample", disco.Issuer);
			var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
			{
				Address = disco.TokenEndpoint,

				ClientAssertion =
				{
					Type = OidcConstants.ClientAssertionTypes.JwtBearer,
					Value = clientToken
				},

				Scope = "scope1"
			});

			if (response.IsError) throw new Exception(response.Error);
			return response;
		}

		string CreateClientToken(SigningCredentials credential, string clientId, string audience)
		{
			var now = DateTime.UtcNow;

			var token = new JwtSecurityToken(
				clientId,
				audience,
				new List<Claim>()
				{
					new Claim(JwtClaimTypes.JwtId, Guid.NewGuid().ToString()),
					new Claim(JwtClaimTypes.Subject, clientId),
					new Claim(JwtClaimTypes.IssuedAt, now.ToEpochTime().ToString(), ClaimValueTypes.Integer64)
				},
				now,
				now.AddMinutes(1),
				credential
			);

			var tokenHandler = new JwtSecurityTokenHandler();
			var clientToken = tokenHandler.WriteToken(token);
			"\n\nClient Authentication Token:".ConsoleGreen();
			Console.WriteLine(token);
			return clientToken;
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
