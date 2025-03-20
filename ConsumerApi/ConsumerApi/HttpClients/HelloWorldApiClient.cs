namespace ConsumerApi.HttpClients
{
	public class HelloWorldApiClient
	{
		public HttpClient Client { get; set; }

		public HelloWorldApiClient(HttpClient client)
		{
			Client = client;
		}
	}
}