using Microsoft.AspNetCore.Mvc;

namespace HelloWorldApi;

[Route("identity")]
[ApiController]
public class IdentityController : ControllerBase
{
    private readonly ILogger<IdentityController> _logger;

    public IdentityController(ILogger<IdentityController> logger)
    {
        _logger = logger;
    }

    // this action simply echoes the claims back to the client
    [HttpGet]
    public ActionResult Get()
    {
	    var responseObject = new
	    {
		    message = "Hello, World!",
		    timestamp = DateTime.Now
	    };

		return new JsonResult(responseObject);
    }
}