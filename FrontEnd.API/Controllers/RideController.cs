using Microsoft.AspNetCore.Mvc;
using OpenTelemetry;
using System.Diagnostics;

namespace FrontEnd.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RideController : ControllerBase
    {
        private readonly ILogger<RideController> _logger;
        private readonly HttpClient _httpClient;

        public RideController(ILogger<RideController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();

        }

        [HttpPost(Name = "BookRide")]
        public async Task<string> BookRide(RideRequest rideRequest)
        {
            Activity.Current?.SetBaggage("CustomerId", rideRequest.CustomerId);
            Activity.Current?.AddEvent(new ActivityEvent("New Booking Recieved"));
            Activity.Current?.SetTag("MyCustomTag1", "Tag1Value");

            _logger.LogInformation("Calling Booking API");
            var response = await _httpClient.PostAsJsonAsync("https://localhost:7292/booking/", rideRequest);

            if(Activity.Current != null)
                return Activity.Current.TraceId.ToString();
            else
                return string.Empty;
        }
    }
}