namespace ConsumerApi
{
	public class IdentityServerClient
	{
		public HttpClient HttpClient { get; set; }

		public IdentityServerClient(HttpClient httpClient)
		{
			HttpClient = httpClient;
		}
	}
}