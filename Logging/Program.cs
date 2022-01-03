// See https://aka.ms/new-console-template for more information

using Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureAppConfiguration(c =>
{
    c.AddJsonFile("appsettings.json", false, true);
});

builder.ConfigureLogging(c =>
{
    var config = c.Services.BuildServiceProvider().GetRequiredService<IConfiguration>();
    c.AddConfiguration(config)
        .AddJsonConsole(j => { j.JsonWriterOptions = new System.Text.Json.JsonWriterOptions() { Indented = true }; });
});
            

using var host = builder.Build();

var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
var jobStarter = new JobStarter(loggerFactory);
await jobStarter.StartProcess();

var config = host.Services.GetRequiredService<IConfiguration>();
config.GetReloadToken().RegisterChangeCallback((o) => Console.WriteLine("Changed"), null);

Console.ReadKey();

await jobStarter.StartProcess();

Console.ReadKey();

await host.RunAsync();
