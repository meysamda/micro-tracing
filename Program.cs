// See https://aka.ms/new-console-template for more information
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.Splunk;

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((hostingContext, loggerConfiguration) => 
    {
        Serilog.Debugging.SelfLog.Enable(Console.Error);
        loggerConfiguration.Enrich.FromLogContext();
        loggerConfiguration.WriteTo.Console();
        loggerConfiguration.WriteTo.Http("http://localhost:5044", null);
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<TraceService>();
        services.AddOpenTelemetry()
            .WithTracing(builder => {
                builder
                    .AddSource("tracing application");
            });
    })
    .Build();

await host.RunAsync();


public class EventData
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class TraceService : BackgroundService
{
    private readonly ILogger<TraceService> _logger;

    public TraceService(ILogger<TraceService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var activity = new Activity("operation 1");
        activity.Start();
        
        var meysam = new EventData { Id = 1, FirstName = "meysam", LastName = "abbasi" };
        _logger.LogInformation("new person created, Person Data: {@Person}", meysam);
        
        activity.Stop();

        await Task.Delay(5000);
    }
}