using client;
using client.EnergyMeter;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

await using var host = CreateHostBuilder(args).Build();

await host.RunAsync();
return;

static WebApplicationBuilder CreateHostBuilder(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddCommandLine(args);
    builder.Configuration.AddEnvironmentVariables();
    builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
    builder.Configuration.AddJsonFile("appsettings.json", false, true);
    builder.Configuration.AddEnvironmentVariables();
    builder.Configuration.AddUserSecrets<Program>();
    builder
        .Host
        .ConfigureServices((_, services) => { ConfigureServices(services); });

    return builder;
}

static void ConfigureServices(IServiceCollection services)
{
    // MQTT
    services.AddHostedService<MqttClient>();
    services.AddSingleton<IMqttMessageHandler, MqttMessageHandler>();
    services.AddTransient<IEnergyReadingHandler, EnergyReadingHandler>();
}