using Microsoft.Azure.Functions.Worker.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Trace;
using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using System;

namespace Ordering.Events
{
    public class Program
    {
        public static void Main()
        {
            AppContext.SetSwitch("Azure.Experimental.EnableActivitySource", true);

            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureLogging(c => c.AddOpenTelemetry(s => s.AddConsoleExporter().IncludeScopes = true).AddSeq().Configure(o => o.ActivityTrackingOptions = ActivityTrackingOptions.SpanId
                                              | ActivityTrackingOptions.TraceId
                                              | ActivityTrackingOptions.ParentId
                                              | ActivityTrackingOptions.Baggage
                                              | ActivityTrackingOptions.Tags))
                .ConfigureServices(s => s.AddOpenTelemetryTracing(c => c.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation(c => c.Filter = (m) => { return m.RequestUri.ToString().EndsWith("events/raw"); })
                    .AddConsoleExporter()
                    .AddJaegerExporter()
                    .AddAzureMonitorTraceExporter(c => c.ConnectionString = "InstrumentationKey=aac2cf64-8c25-4a6f-a2ea-4031c3ef19db;IngestionEndpoint=https://southindia-0.in.applicationinsights.azure.com/")
                    .AddSource("Azure.*")
                    .SetSampler(new AlwaysOnSampler()))
                    .AddHttpClient())
                .Build();

            host.Run();
        }
    }
}