using Microsoft.Extensions.Logging;
using OpenTelemetry;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Logging.Types;

namespace Logging
{
    internal class JobStarter
    {
        private readonly ILogger<JobStarter> _logger;

        public JobStarter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<JobStarter>();
        }

        public async Task StartProcess()
        {
            _logger.LogDebug("Staring Process at {date}",DateTime.UtcNow);

            using (_logger.BeginScope(new Dictionary<string, object>() { { "JobCode", "JC123" } }))
            {
                _logger.LogDebug("Staring Job");

                _logger.LogInformation("Job Finished");
            }

            Person person = new Person() { PersonId = 100, Name = "XYZ" };
            _logger.LogInformation("Stoping Process for {@person}", person);

            _logger.LogHelloWorld(new Person2(100, "XYZ"));
        }
    }
}
