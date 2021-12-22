using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;

namespace Ordering.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;

        private readonly HttpClient _httpClient;

        private readonly ServiceBusClient _serviceBusClient;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, HttpClient httpClient, ServiceBusClient client)
        {
            _logger = logger;
            _httpClient = httpClient;
            _serviceBusClient = client;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            _logger.LogInformation("Calling API");
            await this._httpClient.GetAsync("https://www.google.com");

            //await this._httpClient.GetAsync("http://localhost:7071/api/Function1");

            var sender = _serviceBusClient.CreateSender("testqueue");
            var message = new ServiceBusMessage("Hello");
            await sender.SendMessageAsync(message);

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}