using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Customers.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ILogger<CustomerController> _logger;
        private readonly CustomerDBContext _customerDBContext;

        public CustomerController(ILogger<CustomerController> logger, CustomerDBContext customerDBContext)
        {
            _logger = logger;
            _customerDBContext = customerDBContext;
        }

        [HttpGet]
        [Route("{customerId}")]
        public async Task<Customer> Get(string customerId)
        {
            Activity.Current?.SetTag("MyCustomTag2", "Tag2Value");
            _logger.LogInformation("Getting Customer Info");
            var customer = await _customerDBContext.Customers.FirstOrDefaultAsync(x => x.CustomerUniqueId == customerId);
            return customer;
        }
    }
}