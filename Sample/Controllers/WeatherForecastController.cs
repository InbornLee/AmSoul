using AmSoul.Core.Extensions;
using AmSoul.Core.Interfaces;
using AmSoul.Core.Models;
using AmSoul.Identity.MongoDB.Controllers;
using AmSoul.Identity.MongoDB.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Sample.Models;
using System.Net.Http.Headers;

namespace Sample.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : BaseController
{
    private readonly ILogger<WeatherForecastController> _logger;
    private new readonly IUserService _userService;
    private readonly IMongoCollection<AreaCode> _collection;

    public WeatherForecastController(IUserService userService, ILogger<WeatherForecastController> logger, MongoDbDatabaseSetting settings) : base(userService)
    {
        _userService = userService;
        _logger = logger;
        _collection = MongoDbQueryExtensions.GetCollection<AreaCode>(settings, $"{nameof(AreaCode).ToCamelCase()}s");
    }

    [HttpGet]
    public async Task<IActionResult> GetWeather(string code)
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri("https://restapi.amap.com");
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36");
        //…Ë÷√accept±ÍÕ∑
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var cityCode = code.Trim();
        long number1 = 0;
        bool canConvert = long.TryParse(code, out number1);
        if (!canConvert)
            cityCode = (await GetCityCode(code.Trim())).AdCode;
        var url = $"v3/weather/weatherInfo?key=7f0497b2bc9520a70736095af818d028&city={cityCode}&extensions=all";
        HttpResponseMessage response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        return Ok(result);
    }
    [HttpGet("GetCityCode")]
    public async Task<AreaCode> GetCityCode(string name)
    {
        var result = await _collection.FindAsync(x => x.Name.Contains(name));
        return result.FirstOrDefault();
    }
    //private string GetCityCode(string cityName)
    //{
    //    using var db = new LiteDatabase(@"cityCodes.db");
    //    var collection = db.GetCollection<Citycode>("cityCodes");
    //    var result = collection.FindOne(x => x.Name.Contains(cityName));
    //    return result.AdCode;
    //}

}