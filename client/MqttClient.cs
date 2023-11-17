using System.Text;
using System.Threading.Channels;
using MQTTnet;
using MQTTnet.Client;

namespace client;

public class MqttClient : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IConfiguration _config;
    private readonly IMqttMessageHandler _mqttMessageHandler;
    private readonly MqttFactory _factory = new();

    public MqttClient(ILogger<MqttClient> logger,
        IConfiguration config,
        IMqttMessageHandler mqttMessageHandler)
    {
        _logger = logger;
        _config = config;
        _mqttMessageHandler = mqttMessageHandler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = Channel.CreateBounded<MqttApplicationMessage>(new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
        });

        var mqttClientOptions = new List<MqttClientOptions>();

        var mqttsBroker = _config.GetConnectionString("MqttsBroker");

        if (!string.IsNullOrWhiteSpace(mqttsBroker))
        {
            mqttClientOptions.Add(GetMqttClientOptions(mqttsBroker));
        }

        var mqttClients = new List<IMqttClient>();

        foreach (var option in mqttClientOptions)
            mqttClients.Add(await SetupMqttClient(option, channel.Writer, stoppingToken));

        while (!stoppingToken.IsCancellationRequested)
            await HandleMessage(await channel.Reader.ReadAsync(stoppingToken));

        _logger.LogInformation("{Implementation} disconnecting", GetType().Name);

        foreach (var mqttClient in mqttClients)
        {
            await mqttClient.UnsubscribeAsync(
                new MqttClientUnsubscribeOptions { TopicFilters = new List<string> { "#" } },
                stoppingToken);
            await mqttClient.DisconnectAsync(new MqttClientDisconnectOptions(), stoppingToken);
        }

        _logger.LogInformation("{Implementation} draining queue", GetType().Name);

        channel.Writer.Complete();
        while (channel.Reader.TryRead(out var message))
            await HandleMessage(message);

        _logger.LogInformation("{Implementation} exits", GetType().Name);
        return;

        static MqttClientOptions GetMqttClientOptions(string connectionString)
        {
            var connectionDictionary = connectionString.Split(';')
                .Where(value => !string.IsNullOrEmpty(value))
                .Select(value => value.Split('='))
                .ToDictionary(pair => pair[0], pair => pair[1]);

            var clientId = connectionDictionary["ClientId"] + "-" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var brokerAddress = connectionDictionary["Host"];
            var brokerPort = Convert.ToInt32(connectionDictionary["Port"]);
            var username = connectionDictionary["Username"];
            var password = connectionDictionary["Password"];

            return new MqttClientOptionsBuilder()
                .WithClientId(clientId)
                .WithTcpServer(brokerAddress, brokerPort)
                .WithCredentials(username, password)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(10))
                .WithTlsOptions(o => { o.UseTls(brokerPort == 8883); })
                .Build();
        }
    }

    private async Task<IMqttClient> SetupMqttClient(MqttClientOptions options,
        ChannelWriter<MqttApplicationMessage> channel,
        CancellationToken stoppingToken)
    {
        var mqttClient = _factory.CreateMqttClient();
        var connectionString = _config.GetConnectionString("MqttBroker");
        _logger.LogDebug("MQTT Broker connection string: {ConnectionString}", connectionString);

        mqttClient.DisconnectedAsync += async e =>
        {
            if (e.Exception != null)
                _logger.LogError(e.Exception, "Disconnected from server because {Reason}, attempting reconnect",
                    e.Reason);
            else
                _logger.LogWarning("Disconnected from server because {Reason}, attempting reconnect", e.Reason);

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            try
            {
                await mqttClient.ConnectAsync(options, CancellationToken.None);
            }
            catch
            {
                _logger.LogWarning("Reconnection failed");
            }
        };

        mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            await channel.WriteAsync(e.ApplicationMessage, stoppingToken);
        };

        mqttClient.ConnectedAsync += async _ =>
        {
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("#").Build(), stoppingToken);
        };

        while (!stoppingToken.IsCancellationRequested && !mqttClient.IsConnected)
        {
            try
            {
                await mqttClient.ConnectAsync(options, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not connect to Mqtt broker");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        _logger.LogInformation("{Implementation} Connected", options.ClientId);
        return mqttClient;
    }

    private async Task HandleMessage(MqttApplicationMessage message)
    {
        var topic = message.Topic;
        var payload = message.PayloadSegment == null! ? string.Empty : Encoding.UTF8.GetString(message.PayloadSegment);
        var qos = message.QualityOfServiceLevel;
        var retain = message.Retain;

        _logger.LogDebug("--- MQTT Message ---");
        _logger.LogDebug("Topic: {Topic}", topic);
        _logger.LogDebug("Payload: {Payload}", payload);
        _logger.LogDebug("QoS: {Qos}", qos);
        _logger.LogDebug("Retain: {Retain}", retain);
        _logger.LogDebug("--- End of Message ---");

        await _mqttMessageHandler.HandleMessage(message.Topic, payload);
    }
}