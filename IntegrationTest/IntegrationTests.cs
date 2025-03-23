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
		public async Task PerfomanceTest()
		{
			var consumerToHelloRequestCount = 1000;
			var tasks = GlobalInitFixture.ConsumerContainersPorts.Select(async consumerInstancesPort =>
			{
				int consumerCallsCount = 1;
				await Parallel.ForEachAsync(
					Enumerable.Range(1, consumerCallsCount),
					async (index, cancellationToken) =>
					{
						var httpClient = new HttpClient();
						httpClient.BaseAddress = new Uri($"http://localhost:{consumerInstancesPort}/");
						var response =
							await httpClient.GetStringAsync($"ConsumeHelloWorld/{consumerToHelloRequestCount}");
						var result = JsonConvert.DeserializeObject<List<HelloWorld>>(response);

						Assert.NotNull(result);
						Assert.Equal(consumerToHelloRequestCount, result.Count);
					});
			});
			await Task.WhenAll(tasks);
		}
	}
}