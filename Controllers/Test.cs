using Microsoft.AspNetCore.Mvc;

namespace SprPhone.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController(ILogger<TestController> logger) : ControllerBase
{
    private readonly ILogger<TestController> _logger = logger;

    [HttpGet(Name = "GetTest")]
    public string Get(string name="World")
    {
        return $"Hello, {name}!";
    }
}
