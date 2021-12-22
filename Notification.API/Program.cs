using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Azure;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Request.Body.Peeker;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Azure.Experimental.EnableActivitySource", true);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddAzureClients(builder =>
{
    builder.AddServiceBusClient("Endpoint=sb://wg-sb123.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=uyGx6ag6dMnRAOSZLq68PvgbBtqKOZBO23TdiPIhQg8=");
    builder.AddBlobServiceClient("DefaultEndpointsProtocol=https;AccountName=workgroupdiag344;AccountKey=fD+VAvyAj0jtjqMCRQadAJxqtNkvmIG4pq0V6MocRPyo2t3FQevay30E9HpsQh38jX3Ja5DeSLF4FK3qBQyB/g==;EndpointSuffix=core.windows.net");
});

builder.Logging.ClearProviders().AddOpenTelemetry(c => c.AddConsoleExporter().IncludeScopes = true)
               .AddSeq()
               .AddApplicationInsights("aac2cf64-8c25-4a6f-a2ea-4031c3ef19db")
               .Configure(o => o.ActivityTrackingOptions = ActivityTrackingOptions.SpanId
                                              | ActivityTrackingOptions.TraceId
                                              | ActivityTrackingOptions.ParentId
                                              | ActivityTrackingOptions.Baggage
                                              | ActivityTrackingOptions.TraceState
                                              | ActivityTrackingOptions.TraceFlags
                                              | ActivityTrackingOptions.Tags);

builder.Services.AddOpenTelemetryTracing((Action<TracerProviderBuilder>)(c =>
{
    c.AddAspNetCoreInstrumentation(c => {
        c.Filter = FilterAspNetCoreInstrumentation();
        c.Enrich = EnrichAspNetCoreInstrumentation();
        c.RecordException = true;
    })
    .AddHttpClientInstrumentation(c => {
        c.Filter = FilterHttpInstrumentation();
        c.RecordException = true;
    })
    .AddJaegerExporter()
    .AddConsoleExporter()
    .AddAzureMonitorTraceExporter(c => c.ConnectionString = "InstrumentationKey=aac2cf64-8c25-4a6f-a2ea-4031c3ef19db;IngestionEndpoint=https://southindia-0.in.applicationinsights.azure.com/")
    .AddSource("Azure.*")
    .SetSampler(new AlwaysOnSampler())
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Notification.Processor"));
}));

builder.Services.AddHostedService<NotificationWorkerServiceBus>();

var listener = new ActivityListener
{
    ShouldListenTo = _ => true,
    ActivityStopped = activity =>
    {
        foreach (var (key, value) in activity.Baggage)
        {
            activity.AddTag(key, value);
        }
    }
};
ActivitySource.AddActivityListener(listener);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

Func<HttpRequestMessage, bool> FilterHttpInstrumentation()
{
    return (m) =>
    {
        HashSet<string> noisyCalls = new HashSet<string>();
        noisyCalls.Add("events/raw");
        noisyCalls.Add("v2/track");

        foreach (var call in noisyCalls)
        {
            if (m.RequestUri.ToString().Contains(call))
                return false;
        }

        return true;
    };
}

Func<HttpContext, bool> FilterAspNetCoreInstrumentation()
{
    return (m) =>
    {
        HashSet<string> noisyCalls = new HashSet<string>();
        noisyCalls.Add("swagger");
        noisyCalls.Add("aspnetcore-browser-refresh.js");

        foreach (var call in noisyCalls)
        {
            if (m.Request.Path.ToString().Contains(call))
                return false;
        }

        return true;
    };
}

Action<Activity, string, object> EnrichAspNetCoreInstrumentation()
{
    return async (activity, eventName, rawObject) =>
    {
        if (eventName.Equals("OnStartActivity"))
        {
            if (rawObject is HttpRequest httpRequest)
            {
                foreach (var header in httpRequest.Headers)
                {
                    activity.SetTag($"httpRequestheader.{header.Key}", header.Value);
                }

                activity.SetTag("httpRequest.Body", await httpRequest.PeekBodyAsync());
            }
        }
    };
}
