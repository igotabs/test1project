using ConsumerApi.Models;
using IdentityServerHost;
using Newtonsoft.Json;
using ServiceTests.Fixtures;

namespace ServiceTests
{
	public class ServiceTests : IClassFixture<HostTestFixture>
	{
		private readonly HostTestFixture _fixture;

		public ServiceTests(HostTestFixture fixture)
		{
			_fixture = fixture;
		}
		[Fact]
		public async Task CallConsumeHelloWorldWithCountOne() 
		{
			var consumerToHelloRequestCount = 10;
			var tasks = _fixture.ConsumerClients.Select(async client =>
			{
				int consumerCallsCount = 1;
				for (int i = 0; i < consumerCallsCount; i++)
				{

					var response =
						await client.GetStringAsync($"ConsumeHelloWorld/{consumerToHelloRequestCount}");
					var result = JsonConvert.DeserializeObject<List<HelloWorld>>(response);

					Assert.NotNull(result);
					Assert.Equal(consumerToHelloRequestCount, result.Count);
				}
			});

			await Task.WhenAll(tasks);
		}
	}
}