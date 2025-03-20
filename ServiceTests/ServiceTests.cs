using ConsumerApi.Models;
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
			var response = await _fixture.ConsumerClient.GetStringAsync($"ConsumeHelloWorld?count=1");
			var result = JsonConvert.DeserializeObject<List<HelloWorld>>(response);

			Assert.NotNull(result);
			Assert.Equal(1, result.Count);
		}
	}
}