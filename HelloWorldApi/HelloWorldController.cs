﻿using Microsoft.AspNetCore.Mvc;
using RedLockNet;

namespace HelloWorldApi;

[Route("HelloWorld")]
[ApiController]
public class HelloWorldController : ControllerBase
{
	public const string ConfigUpdateResourceLock = "configUpdateResourceLock";
	private readonly TimeSpan _redlockExpiration = TimeSpan.FromSeconds(30);
	private readonly IDistributedLockFactory _distributedLockFactory;
	private readonly ILogger<HelloWorldController> _logger;

    public HelloWorldController(
	    IDistributedLockFactory distributedLockFactory,
	    ILogger<HelloWorldController> logger)
    {
	    _distributedLockFactory = distributedLockFactory;
	    _logger = logger;
    }

    // this action simply echoes the claims back to the client
    [HttpGet]
    public async Task<ActionResult> Get()
    {
	    await using var redlock = await _distributedLockFactory.CreateLockAsync(
		    ConfigUpdateResourceLock,
		    _redlockExpiration);
	    if (!redlock.IsAcquired)
		    return null;

		var responseObject = new
	    {
		    message = "Hello, World!",
		    timestamp = DateTime.Now
	    };

		return new JsonResult(responseObject);
    }
}