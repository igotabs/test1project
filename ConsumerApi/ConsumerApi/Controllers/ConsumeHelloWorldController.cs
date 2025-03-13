using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Duende.IdentityModel;
using Duende.IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RedLockNet;
using StackExchange.Redis;

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
			_identityServerBaseUrl = configuration["IdentityServer:BaseUrl"];
			_helloWorldApiBaseUrl = configuration["HelloWorldApi:BaseUrl"];
			_logger = logger;
		}

		[HttpGet(Name = "GetWeatherForecast")]
		public async Task<HelloWorld?> Get()
		{
			// would normally load from a secure data store
			string rsaKey = """
{
    "d":"GmiaucNIzdvsEzGjZjd43SDToy1pz-Ph-shsOUXXh-dsYNGftITGerp8bO1iryXh_zUEo8oDK3r1y4klTonQ6bLsWw4ogjLPmL3yiqsoSjJa1G2Ymh_RY_sFZLLXAcrmpbzdWIAkgkHSZTaliL6g57vA7gxvd8L4s82wgGer_JmURI0ECbaCg98JVS0Srtf9GeTRHoX4foLWKc1Vq6NHthzqRMLZe-aRBNU9IMvXNd7kCcIbHCM3GTD_8cFj135nBPP2HOgC_ZXI1txsEf-djqJj8W5vaM7ViKU28IDv1gZGH3CatoysYx6jv1XJVvb2PH8RbFKbJmeyUm3Wvo-rgQ",
    "dp":"YNjVBTCIwZD65WCht5ve06vnBLP_Po1NtL_4lkholmPzJ5jbLYBU8f5foNp8DVJBdFQW7wcLmx85-NC5Pl1ZeyA-Ecbw4fDraa5Z4wUKlF0LT6VV79rfOF19y8kwf6MigyrDqMLcH_CRnRGg5NfDsijlZXffINGuxg6wWzhiqqE",
    "dq":"LfMDQbvTFNngkZjKkN2CBh5_MBG6Yrmfy4kWA8IC2HQqID5FtreiY2MTAwoDcoINfh3S5CItpuq94tlB2t-VUv8wunhbngHiB5xUprwGAAnwJ3DL39D2m43i_3YP-UO1TgZQUAOh7Jrd4foatpatTvBtY3F1DrCrUKE5Kkn770M",
    "e":"AQAB",
    "kid":"ZzAjSnraU3bkWGnnAqLapYGpTyNfLbjbzgAPbbW2GEA",
    "kty":"RSA",
    "n":"wWwQFtSzeRjjerpEM5Rmqz_DsNaZ9S1Bw6UbZkDLowuuTCjBWUax0vBMMxdy6XjEEK4Oq9lKMvx9JzjmeJf1knoqSNrox3Ka0rnxXpNAz6sATvme8p9mTXyp0cX4lF4U2J54xa2_S9NF5QWvpXvBeC4GAJx7QaSw4zrUkrc6XyaAiFnLhQEwKJCwUw4NOqIuYvYp_IXhw-5Ti_icDlZS-282PcccnBeOcX7vc21pozibIdmZJKqXNsL1Ibx5Nkx1F1jLnekJAmdaACDjYRLL_6n3W4wUp19UvzB1lGtXcJKLLkqB6YDiZNu16OSiSprfmrRXvYmvD8m6Fnl5aetgKw",
    "p":"7enorp9Pm9XSHaCvQyENcvdU99WCPbnp8vc0KnY_0g9UdX4ZDH07JwKu6DQEwfmUA1qspC-e_KFWTl3x0-I2eJRnHjLOoLrTjrVSBRhBMGEH5PvtZTTThnIY2LReH-6EhceGvcsJ_MhNDUEZLykiH1OnKhmRuvSdhi8oiETqtPE",
    "q":"0CBLGi_kRPLqI8yfVkpBbA9zkCAshgrWWn9hsq6a7Zl2LcLaLBRUxH0q1jWnXgeJh9o5v8sYGXwhbrmuypw7kJ0uA3OgEzSsNvX5Ay3R9sNel-3Mqm8Me5OfWWvmTEBOci8RwHstdR-7b9ZT13jk-dsZI7OlV_uBja1ny9Nz9ts",
    "qi":"pG6J4dcUDrDndMxa-ee1yG4KjZqqyCQcmPAfqklI2LmnpRIjcK78scclvpboI3JQyg6RCEKVMwAhVtQM6cBcIO3JrHgqeYDblp5wXHjto70HVW6Z8kBruNx1AH9E8LzNvSRL-JVTFzBkJuNgzKQfD0G77tQRgJ-Ri7qu3_9o1M4"
}
""";

			var jwk = new JsonWebKey(rsaKey);
			var response = await RequestTokenAsync(new SigningCredentials(jwk, "RS256"));
			response.Show();

			var items = await CallServiceAsync(response.AccessToken);

			return items;
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
			var response = await httpClient.GetStringAsync("identity");

			"\n\nService claims:".ConsoleGreen();
			Console.WriteLine(response.PrettyPrintJson());
			var result = JsonConvert.DeserializeObject< HelloWorld>(response);

			return result;
		}
	}
}
