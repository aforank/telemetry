using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Request.Body.Peeker;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Newtonsoft;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var redisConfig = builder.Configuration.GetSection("Redis").Get<RedisConfiguration>();
var connection = ConnectionMultiplexer.Connect(builder.Configuration.GetValue<string>("AzureRedisConnectionString"));
builder.Services.AddSingleton(connection);

builder.Logging.ClearProviders().AddOpenTelemetry(c => c.AddConsoleExporter().IncludeScopes = true)
               .AddSeq()
               .AddApplicationInsights(builder.Configuration.GetValue<string>("AppInsightsInstrumentationKey"))
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
    .AddRedisInstrumentation(connection, c => c.SetVerboseDatabaseStatements = true)
    .AddJaegerExporter()
    .AddConsoleExporter()
    .AddAzureMonitorTraceExporter(c => c.ConnectionString = builder.Configuration.GetValue<string>("ApplicationInsightsConnectionString"))
    .AddSource("Azure.*")
    .SetSampler(new AlwaysOnSampler())
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Drivers.API"));
}));

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