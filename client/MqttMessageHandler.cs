using client.EnergyMeter;

namespace client;

public interface IMqttMessageHandler
{
    Task HandleMessage(string topic, string payload);
}

public class MqttMessageHandler : IMqttMessageHandler
{
    private readonly ILogger<MqttMessageHandler> _logger;
    private readonly IEnergyReadingHandler _energyReadingHandler;

    public MqttMessageHandler(ILogger<MqttMessageHandler> logger, IEnergyReadingHandler energyReadingHandler)
    {
        _logger = logger;
        _energyReadingHandler = energyReadingHandler;
    }

    public async Task HandleMessage(string topic, string payload)
    {
        var topicParts = topic.Split("/");

        foreach (var topicPrefix in topicParts)
        {
            if (topicPrefix != EnergyReadingHandler.TopicPrefix) continue;

            _logger.LogTrace("Topic prefix: {Prefix}", topicPrefix);

            try
            {
                await _energyReadingHandler.HandleMessage(topic, payload)!;
                return;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Error handling message {Payload} on {Topic}", topic, payload);
            }
        }
    }
}