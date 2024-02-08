// See https://aka.ms/new-console-template for more information
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.Span;

var host = Host.CreateDefaultBuilder(args)    
    .ConfigureServices((hostContext, services) =>
    {
        services.AddOpenTelemetry()
            .ConfigureResource(config => {
                // config.AddTelemetrySdk();
            })
            .WithTracing(config => {
                config.AddSource("Worker");
                // config.SetResourceBuilder(ResourceBuilder.CreateDefault()
                //     .AddService("MyDemoService"));
                config.AddConsoleExporter();
            });

        services.AddHostedService<Worker>();
    })
    // .ConfigureLogging(config => {
    //     config.AddSerilog();
    //     config.AddOpenTelemetry(options => {
    //         // options.IncludeScopes = true;
    //         // options.IncludeFormattedMessage = true;
    //         // options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MyDemoService"));
    //         options.AddConsoleExporter();
    //     });
    // })
    .UseSerilog((hostingContext, config) => 
    {
        Serilog.Debugging.SelfLog.Enable(Console.Error);
        config.Enrich.FromLogContext();
        config.Enrich.WithSpan(new SpanOptions { IncludeOperationName = true, IncludeTags = true, IncludeTraceFlags = true });
        config.Enrich.WithProperty("custom-field", "test for tracing");
        config.WriteTo.Console();
        // config.WriteTo.EventCollector("http://localhost:8088", "98bea738-3fc1-4c6b-87ba-b80d7637b05c");

        // loggerConfiguration.WriteTo.Http("http://localhost:5044", null);
    })
    .Build();

await host.RunAsync();


public class EventData
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class Worker : BackgroundService
{
    private static ActivitySource source = new ActivitySource("Worker");

    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = source.StartActivity("operation 1");
        
        var meysam = new EventData { Id = 1, FirstName = "meysam", LastName = "abbasi" };
        _logger.LogInformation("new person created, Person Data: {@Person}", meysam);


        // await Task.Delay(5000);

    }
}