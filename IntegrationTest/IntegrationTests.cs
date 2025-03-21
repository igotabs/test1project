using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using IntegrationTest.Fixtures;
using IntegrationTest.Utils;

namespace IntegrationTest
{
	public class IntegrationTests
	{
		[Fact]
		public async Task CallConsumeHelloWorldWithCountOne()
		{
			await Task.Delay(1000);
			var count = 10;
			foreach (var consumerPort in GlobalInitFixture.ConsumerContainresPorts)
			{
				var httpClient = new HttpClient();
				httpClient.BaseAddress = new Uri($"http://localhost:{consumerPort}/");
				//warm-up
				var response = await httpClient.GetStringAsync($"ConsumeHelloWorld?count={count}");
				var result = JsonConvert.DeserializeObject<List<HelloWorld>>(response);

				Assert.NotNull(result);
				Assert.Equal(count, result.Count);
			}
		}
	}
}