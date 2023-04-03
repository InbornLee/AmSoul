using AmSoul.Identity.MongoDB.Controllers;
using AmSoul.Identity.MongoDB.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Sample.Models;

namespace Sample.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : BaseController
{
    private static readonly string[] Summaries = new[]
    {
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IUserService _userService;
    public WeatherForecastController(IUserService userService, ILogger<WeatherForecastController> logger) : base(userService)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get() => Enumerable.Range(1, 5).Select(index => new WeatherForecast
    {
        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        TemperatureC = Random.Shared.Next(-20, 55),
        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
    })
        .ToArray();
}