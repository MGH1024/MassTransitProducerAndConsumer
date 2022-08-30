using Contract.Config;
using Contract.Contracts;
using Contract.Services;
using Contract.Utility;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;


var environmentConfiguration = new ConfigurationBuilder()
                     .AddEnvironmentVariables()
                     .Build();

var environment = Environment.GetEnvironmentVariable("DOTNETCORE_ENVIRONMENT");


var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();


Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(environmentConfiguration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();



IHost host = Host.CreateDefaultBuilder()
    .ConfigureServices((hostContext, services) =>
    {
        //options and services
        services.Configure<RabbitMq>(option =>
           configuration.GetSection(nameof(RabbitMq)).Bind(option));
        services.AddTransient<ISenderService, SenderService>();

        var rabbitMq = configuration.GetSection(nameof(RabbitMq)).Get<RabbitMq>().ConsumerConnection.HostAddress;

        //masstransit
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(rabbitMq);
                cfg.ConfigureEndpoints(ctx);
                cfg.MessageTopology.SetEntityNameFormatter(new ExchangeNameFormatter());
            });
        });

    })
    .UseSerilog()
    .Build();

try
{
    Log.Information("self Producer started...");
    while (true)
    {
        await Task.Delay(1);
        var senderService = host.Services.GetRequiredService<ISenderService>();
        var guid = Guid.NewGuid();
        var customer = new Customer
        {
            Id = guid,
            Name = $"Name with Id :+{guid}"
        };
        await senderService.Publish(customer);
        Log.Information($"{customer.Name} published!");
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "Error In Starting self producer...");
}
finally
{
    Log.CloseAndFlush();
}
