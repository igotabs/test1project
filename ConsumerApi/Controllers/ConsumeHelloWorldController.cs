using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
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
			_identityServerBaseUrl = configuration["IdentityServer:BaseUrl"] ??
			                         throw new ArgumentNullException(nameof(_identityServerBaseUrl));
			_helloWorldApiBaseUrl = configuration["HelloWorldApi:BaseUrl"] ??
			                        throw new ArgumentNullException(nameof(_helloWorldApiBaseUrl));
			_tokenService = tokenService;
			_helloWorldApiClient = helloWorldApiClient;
		}

		[HttpGet("{count}", Name = "GetHelloWorld")]
		public async Task<ActionResult<IEnumerable<HelloWorld?>>> Get(int count = 1)
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

			var token = await _tokenService.GetAccessTokenAsync();
			_helloWorldApiClient.Client.SetBearerToken(token);
			var results = new ConcurrentBag<HelloWorld>();
			var errors = new ConcurrentBag<(HttpStatusCode StatusCode, string ErrorMessage)>();

			await Parallel.ForEachAsync(
				Enumerable.Range(1, count),
				async (index, cancellationToken) =>
				{
					try
					{
						var item = await serviceRetryPolicy.ExecuteAsync(() => CallServiceAsync());
						if (item != null) results.Add(item);
					}
					catch (HttpRequestException ex) when (ex.StatusCode.HasValue)
					{
						errors.Add((ex.StatusCode.Value, ex.Message));
					}
					catch (Exception ex)
					{
						errors.Add((HttpStatusCode.InternalServerError, ex.Message));
					}
				}
			);
			if (errors.IsEmpty)
				return results.ToList();

			var errorDetails = errors.Select(e => $"Status {e.StatusCode}: {e.ErrorMessage}");
			return StatusCode(500, new
			{
				Message = "Partial success with errors.",
				Results = results.ToList(),
				Errors = errorDetails
			});
		}

		async Task<HelloWorld?> CallServiceAsync()
		{
			try
			{
				var response = await _helloWorldApiClient.Client.GetStringAsync($"HelloWorld");
				var result = JsonConvert.DeserializeObject<HelloWorld>(response);
				return result;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}
	}
}