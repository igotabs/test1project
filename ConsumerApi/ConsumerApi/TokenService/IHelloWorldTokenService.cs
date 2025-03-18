namespace ConsumerApi.TokenService;

public interface IHelloWorldTokenService
{
    Task<string> GetAccessTokenAsync();
}