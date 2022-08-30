using Contract;
using Contract.Bus;
using Contract.Config;
using Contract.LatestConsumer;
using Contract.Utility;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;


var environmentConfiguration = new ConfigurationBuilder()
                     .AddEnvironmentVariables()
                     .Build();

var environment = Environment.GetEnvironmentVariable("DOTNETCORE_ENVIRONMENT");


var configuration = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.{environment}.json", false, true)
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

        var rabbitMq = configuration.GetSection(nameof(RabbitMq)).Get<RabbitMq>();

        services.AddMassTransit<ICustomerBusControl>(config =>
        {
            config.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(rabbitMq.ProducerConnection.HostAddress, h =>
                {
                    h.UseCluster(c =>
                    {
                        foreach (var serverUrl in rabbitMq.ClusterServers)
                            c.Node(serverUrl);
                    });
                    h.ConfigureBatchPublish(option =>
                    {
                        option.Enabled = true;
                        option.MessageLimit = 100;
                        option.SizeLimit = 200000;
                        option.Timeout = TimeSpan.FromMilliseconds(2);
                    });
                    h.PublisherConfirmation = false;
                });
                cfg.MessageTopology.SetEntityNameFormatter(new ExchangeNameFormatter());
            });
        });



        services.AddMassTransit(config =>
        {
            config.AddConsumer<CustomerConsumer>();

            config.UsingRabbitMq((ctx, cfg) =>
            {

                cfg.Host(rabbitMq.ConsumerConnection.HostAddress,
                    h => h.UseCluster(c =>
                    {
                        foreach (var serverUrl in rabbitMq.ClusterServers)
                            c.Node(serverUrl);
                    })
                );

                if (environment == "Development")
                {
                    cfg.PrefetchCount = 2;
                    cfg.ConcurrentMessageLimit = 1;
                }

                cfg.ConfigureEndpoints(ctx);
                cfg.MessageTopology.SetEntityNameFormatter(new ExchangeNameFormatter());
            });



        });
    })
    //.UseWindowsService()
    .UseSerilog()
    .Build();



try
{
    Log.Information("Receipt Payment Management Worker starting up ...");
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Error In Starting Receipt Payment Management Worker");
}
finally
{
    Log.CloseAndFlush();
}
