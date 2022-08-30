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

        services.AddMassTransit(config =>
        {
            config.AddConsumer<PersonConsumer>();

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