namespace HelloWorldApi.Tools;

public class RequestInterceptorMiddleware
{
	private readonly RequestDelegate _next;

	public RequestInterceptorMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		// Code to execute before the next middleware
		Console.WriteLine("Incoming request: " + context.Request.Path);

		// Call the next middleware in the pipeline
		await _next(context);

		// Code to execute after the next middleware
		Console.WriteLine("Outgoing response: " + context.Response.StatusCode);
	}
}