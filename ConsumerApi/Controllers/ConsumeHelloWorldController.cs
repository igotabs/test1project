using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
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

namespace ConsumerApi.Controllers
{
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
		public async Task<ActionResult<IEnumerable<HelloWorld?>>> Get(int count = 1)
		{
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
							  var item = await CallServiceAsync(token);
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
		async Task<HelloWorld?> CallServiceAsync(string token)
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
