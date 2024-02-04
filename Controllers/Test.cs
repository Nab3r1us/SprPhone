using Microsoft.AspNetCore.Mvc;

namespace SprPhone.Controllers;

[Route("[controller]")]
public class TestController(ILogger<TestController> logger) : ControllerBase
{
    private readonly ILogger<TestController> _logger = logger;

    [HttpGet]
    public string Get(string name="World")
    {
        return $"Hello, {name}!";
    }
}
