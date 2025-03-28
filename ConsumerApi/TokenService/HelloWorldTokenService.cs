﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ConsumerApi.HttpClients;
using ConsumerApi.Tools;
using Duende.IdentityModel;
using Duende.IdentityModel.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace ConsumerApi.TokenService;

public class HelloWorldTokenService : IHelloWorldTokenService
{
    private readonly IMemoryCache _cache;
    private readonly IdentityServerClient _identityServerClient;

    private readonly string _identityServerBaseUrl;

    public HelloWorldTokenService(
        IMemoryCache cache,
        IConfiguration configuration,
        IdentityServerClient identityServerClient)
    {
        _identityServerBaseUrl = configuration["IdentityServer:BaseUrl"] ?? throw new ArgumentNullException(nameof(_identityServerBaseUrl));
        _cache = cache;
        _identityServerClient = identityServerClient;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        const string cacheKey = "AccessToken";

        if (_cache.TryGetValue(cacheKey, out string? accessToken))
        {
            return accessToken!;
        }
        var jwk = new JsonWebKey(Constants.RsaKey);
        var tokenResponse = await RequestTokenAsync(new SigningCredentials(jwk, "RS256"));

        accessToken = tokenResponse.AccessToken;

        var cacheDuration = TimeSpan.FromSeconds(tokenResponse.ExpiresIn - 60);
        _cache.Set(cacheKey, accessToken, cacheDuration);

        return accessToken!;
    }

    async Task<TokenResponse> RequestTokenAsync(SigningCredentials signingCredentials)
    {
        var disco = await _identityServerClient.HttpClient.GetDiscoveryDocumentAsync();
        if (disco.IsError) throw new Exception(disco.Error);

        var clientToken = CreateClientToken(signingCredentials, Common.Constants.ClientId, disco.Issuer!);
        var response = await _identityServerClient.HttpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = disco.TokenEndpoint,

            ClientAssertion =
            {
                Type = OidcConstants.ClientAssertionTypes.JwtBearer,
                Value = clientToken
            },

            Scope = "scope1"
        });
        response.Show();
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
}