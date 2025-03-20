
namespace ConsumerApi
{
	public class HelloWorldApiClient
	{
		public HttpClient Client { get; set; }

		public HelloWorldApiClient(HttpClient client)
		{
			this.Client = client;
		}
	}
}