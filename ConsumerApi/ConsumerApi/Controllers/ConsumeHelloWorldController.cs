using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
        private readonly string? _helloWorldApiBaseUrl;

        public ConsumeHelloWorldController(
            IConfiguration configuration,
            IHelloWorldTokenService tokenService)
        {
            _identityServerBaseUrl = configuration["IdentityServer:BaseUrl"] ?? throw new ArgumentNullException(nameof(_identityServerBaseUrl));
            _helloWorldApiBaseUrl = configuration["HelloWorldApi:BaseUrl"] ?? throw new ArgumentNullException(nameof(_helloWorldApiBaseUrl));
            _tokenService = tokenService;
        }

        [HttpGet(Name = "GetHelloWorld")]
        public async Task<IEnumerable<HelloWorld?>> Get([FromQuery] int count = 1)
        {
            var token = await _tokenService.GetAccessTokenAsync();

            var results = new ConcurrentBag<HelloWorld>();

            await Parallel.ForEachAsync(
                Enumerable.Range(1, count),
                async (index, cancellationToken) =>
                {
                    var item = await CallServiceAsync(token);
                    if (item != null) results.Add(item);
                }
            );

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
            httpClient.BaseAddress = new Uri(_helloWorldApiBaseUrl!);
            httpClient.SetBearerToken(token);
            var response = await httpClient.GetStringAsync($"HelloWorld");

            Console.WriteLine(response.PrettyPrintJson());
            var result = JsonConvert.DeserializeObject<HelloWorld>(response);

            return result;
        }
    }
}
