using Azure.Monitor.OpenTelemetry.Exporter;
using Customers.API;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Request.Body.Peeker;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<CustomerDBContext>(options =>
  options.UseSqlServer(builder.Configuration.GetConnectionString("CustomerDbContext")));

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
    .AddEntityFrameworkCoreInstrumentation(c => {
        c.SetDbStatementForText = true;
        c.SetDbStatementForStoredProcedure = true;
        })
    .AddJaegerExporter()
    .AddConsoleExporter()
    .AddAzureMonitorTraceExporter(c => c.ConnectionString = "InstrumentationKey=aac2cf64-8c25-4a6f-a2ea-4031c3ef19db;IngestionEndpoint=https://southindia-0.in.applicationinsights.azure.com/")
    .AddSource("Azure.*")
    .SetSampler(new AlwaysOnSampler())
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Customer.API"));
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

//Initialize DB
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<CustomerDBContext>();
    context.Database.EnsureCreated();
    DbInitializer.Initialize(context);
}

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
