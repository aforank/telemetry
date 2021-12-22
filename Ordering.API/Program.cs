using OpenTelemetry.Logs;
using OpenTelemetry.Trace;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Azure.Experimental.EnableActivitySource", true);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.ClearProviders().AddOpenTelemetry(c => c.AddConsoleExporter().IncludeScopes = true)
               .AddSeq()
               .AddApplicationInsights("aac2cf64-8c25-4a6f-a2ea-4031c3ef19db")
               .Configure(o => o.ActivityTrackingOptions = ActivityTrackingOptions.SpanId
                                              | ActivityTrackingOptions.TraceId
                                              | ActivityTrackingOptions.ParentId
                                              | ActivityTrackingOptions.TraceState
                                              | ActivityTrackingOptions.TraceFlags
                                              | ActivityTrackingOptions.Tags);

builder.Services.AddOpenTelemetryTracing((Action<TracerProviderBuilder>)(c =>
{
    c.AddAspNetCoreInstrumentation(c => c.Filter = FilterAspNetCoreInstrumentation())
    .AddHttpClientInstrumentation(c => c.Filter = FilterHttpInstrumentation() )
    .AddJaegerExporter()
    .AddZipkinExporter()
    .AddOtlpExporter()
    //.AddOtlpExporter(option =>
    // {
    //    option.Endpoint = new Uri("https://api.honeycomb.io");
    //    option.Headers = $"x-honeycomb-team=cea93c4b585c4e61afc636d027b1f2d3,x-honeycomb-dataset=Telem";
    // })
    .AddAzureMonitorTraceExporter(c => c.ConnectionString = "InstrumentationKey=aac2cf64-8c25-4a6f-a2ea-4031c3ef19db;IngestionEndpoint=https://southindia-0.in.applicationinsights.azure.com/")
    .AddSource("Azure.*")
    .SetSampler(new AlwaysOnSampler());
}));

//2
builder.Services.AddHttpClient();
builder.Services.AddAzureClients(builder =>
{
    builder.AddServiceBusClient("Endpoint=sb://wg-sb123.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=uyGx6ag6dMnRAOSZLq68PvgbBtqKOZBO23TdiPIhQg8=");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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