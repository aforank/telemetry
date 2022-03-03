using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Logging.Types;

namespace Logging
{
    public static partial class LogTemplates
    {
        [LoggerMessage(0, LogLevel.Information, "Writing hello world response to {Person}")]
        public static partial void LogHelloWorld(this ILogger logger, Person2 person);
    }
}
