using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using ConsumerApi.HttpClients;
using ConsumerApi.Models;
using ConsumerApi.TokenService;
using ConsumerApi.Tools;
using Duende.IdentityModel;
using Duende.IdentityModel.Client;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;

namespace ConsumerApi.Controllers
{

	// Thread-safe random generator
	static class RandomInterval
	{
		private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

		public static TimeSpan GetRandomDelay(int minSeconds, int maxSeconds)
		{
			var byteArray = new byte[4];
			_rng.GetBytes(byteArray);
			int range = maxSeconds - minSeconds;
			int randomValue = BitConverter.ToInt32(byteArray, 0) & int.MaxValue; // Force positive
			return TimeSpan.FromSeconds(minSeconds + (randomValue % range));
		}
	}


	[ApiController]
    [Route("[controller]")]
    public class ConsumeHelloWorldController : ControllerBase
    {
        private readonly string? _identityServerBaseUrl;
        private readonly IHelloWorldTokenService _tokenService;
        private readonly HelloWorldApiClient _helloWorldApiClient;
        private readonly string? _helloWorldApiBaseUrl;

        public ConsumeHelloWorldController(
            IConfiguration configuration,
            IHelloWorldTokenService tokenService,
            HelloWorldApiClient helloWorldApiClient)
        {
            _identityServerBaseUrl = configuration["IdentityServer:BaseUrl"] ?? throw new ArgumentNullException(nameof(_identityServerBaseUrl));
            _helloWorldApiBaseUrl = configuration["HelloWorldApi:BaseUrl"] ?? throw new ArgumentNullException(nameof(_helloWorldApiBaseUrl));
            _tokenService = tokenService;
            _helloWorldApiClient = helloWorldApiClient;
        }

        [HttpGet("{count}", Name = "GetHelloWorld")]
        public async Task<IEnumerable<HelloWorld?>> Get(int count = 1)
        {
	        AsyncRetryPolicy<string> tokenRetryPolicy = Policy<string>
				.Handle<Exception>()
		        .OrResult(string.IsNullOrEmpty)
				.WaitAndRetryAsync(3,
			        retryAttempt => RandomInterval.GetRandomDelay(1, 2)); // 1-3 seconds

	        AsyncRetryPolicy<HelloWorld?> serviceRetryPolicy = Policy<HelloWorld?>
		        .Handle<Exception>()
		        .OrResult(r => r == null)
		        .WaitAndRetryAsync(3,
			        retryAttempt => RandomInterval.GetRandomDelay(1, 2)); // 2-5 seconds




			var token = await tokenRetryPolicy.ExecuteAsync(() => _tokenService.GetAccessTokenAsync());

            var results = new ConcurrentBag<HelloWorld>();

            await Parallel.ForEachAsync(
                Enumerable.Range(1, count),
                async (index, cancellationToken) =>
                {
                    var item = await serviceRetryPolicy.ExecuteAsync(() => CallServiceAsync(token));
                    if (item != null) results.Add(item);
                }
            );

            return results.ToList();
        }
        async Task<HelloWorld?> CallServiceAsync(string token)
        {
			_helloWorldApiClient.Client.SetBearerToken(token);
            var response = await _helloWorldApiClient.Client.GetStringAsync($"HelloWorld");

            Console.WriteLine(response.PrettyPrintJson());
            var result = JsonConvert.DeserializeObject<HelloWorld>(response);

            return result;
        }
    }
}
