using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using ConsumerApi.Models;

namespace IntegrationTest
{
	public class IntegrationTests
	{
		[Fact]
		public async Task CallConsumeHelloWorldWithCountOne()
		{
			var httpClient = new HttpClient();
			httpClient.BaseAddress = new Uri("http://localhost:5003/");
			//warm-up
			await Task.Delay(10000);
			var response = await httpClient.GetStringAsync($"ConsumeHelloWorld?count=1");
			var result = JsonConvert.DeserializeObject<List<HelloWorld>>(response);

			Assert.NotNull(result);
			Assert.Equal(1, result.Count);
		}
	}
}