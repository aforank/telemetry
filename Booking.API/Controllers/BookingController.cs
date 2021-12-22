using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Booking.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly ILogger<BookingController> _logger;
        private readonly HttpClient _httpClient;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly MongoClient mongoClient;

        public BookingController(ILogger<BookingController> logger, IHttpClientFactory httpClientFactory, ServiceBusClient client, MongoClient mongoClient)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _serviceBusClient = client;
            this.mongoClient = mongoClient;
        }

        [HttpPost]
        public async Task<BookingResponse> Book(BookingRequest bookingRequest)
        {
            _logger.LogInformation("Fetching Customer Details");
            var customerInfo = await _httpClient.GetFromJsonAsync<Customer>("https://localhost:7249/customer/CX111000");
            var driverInfo = await _httpClient.GetFromJsonAsync<Driver>("https://localhost:7026/driver");

            var database = this.mongoClient.GetDatabase("rides");
            var collection = database.GetCollection<BookingDetails>("booking");


            BookingDetails bookingDetails = new BookingDetails();
            bookingDetails.Customer = customerInfo;
            bookingDetails.Driver = driverInfo;
            bookingDetails.Price = EstimateRide.EstimatePrice();

            await collection.InsertOneAsync(bookingDetails);

            var sender = _serviceBusClient.CreateSender("payments");
            var message = new ServiceBusMessage(Newtonsoft.Json.JsonConvert.SerializeObject(bookingDetails));
            await sender.SendMessageAsync(message);

            Activity.Current?.AddEvent(new ActivityEvent("Ride Booked"));

            return new BookingResponse(bookingDetails);
        }
    }
}