using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;
using System.Text.Json.Serialization;

namespace Drivers.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DriverController : ControllerBase
    {
        private readonly ILogger<DriverController> _logger;
        private readonly ConnectionMultiplexer redisCacheClient;

        public DriverController(ILogger<DriverController> logger, ConnectionMultiplexer redisCacheClient)
        {
            _logger = logger;
            this.redisCacheClient = redisCacheClient;
        }

        [HttpGet]
        public async Task<Driver> Get()
        {
            var cachedKey = "drivers";
            Driver driver = null;

            var db = this.redisCacheClient.GetDatabase(0);

            if (!(await db.KeyExistsAsync(cachedKey)))
            {
                driver = new Driver();
                driver.Name = "Gabby";
                driver.CarNumber = "GabDL14C5001by";
                string driverString = JsonConvert.SerializeObject(driver);
                await db.StringSetAsync(cachedKey, driverString, TimeSpan.FromSeconds(10));
            }
            else
            {
                //get cached value
                var driverStr = await db.StringGetAsync(cachedKey);
                driver = JsonConvert.DeserializeObject<Driver>(driverStr);
            }

            return driver;
        }
    }
}