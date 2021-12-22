using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ordering.Events
{
    public class Function1
    {
        private readonly ILogger _logger;
        private readonly HttpClient httpClient;

        public Function1(ILoggerFactory loggerFactory, HttpClient httpClient)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
            this.httpClient = httpClient;
        }

        [Function("Function1")]
        public async Task Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req)
        {
            _logger.LogInformation($"Recieved Http Request from Function1");

            await this.httpClient.GetAsync("https://www.facebook.com");
        }

        [Function("Function2")]
        public async Task RunTry([ServiceBusTrigger("testqueue", Connection = "SbConnection")] string myQueueItem)
        {
            _logger.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");

            await this.httpClient.GetAsync("https://www.facebook.com");
        }
    }
}
