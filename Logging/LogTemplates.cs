using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging
{
    public static partial class LogTemplates
    {
        [LoggerMessage(0, LogLevel.Information, "Writing hello world response to {person}")]
        public static partial void LogHelloWorld(this ILogger logger, Person person);
    }
}
