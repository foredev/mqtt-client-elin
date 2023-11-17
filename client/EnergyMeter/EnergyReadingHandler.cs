namespace client.EnergyMeter;

public interface IEnergyReadingHandler
{
    Task HandleMessage(string topic, string payload);
}

public class EnergyReadingHandler : IEnergyReadingHandler
{
    public const string TopicPrefix = "foredev";
    private const string OperationEnergyReading = "EnergyReader";
    private const string OperationLog = "log";

    public async Task HandleMessage(string topic, string payload)
    {
        var topicComponents = topic.Split("/");
        if (topicComponents.Length != 3 && topicComponents.Length != 4)
        {
            return;
        }

        var operation = topicComponents.Length == 3 ? topicComponents[1] : topicComponents[3];
        var id = topicComponents[2];

        switch (operation)
        {
            case OperationEnergyReading:
                await HandleEnergyReader(id, payload);
                break;
            case OperationLog:
                break;
        }
    }

    private Task HandleEnergyReader(string id, string payload)
    {
        var data = EnergyMeterDataParser.Parse(payload);

        // ...

        return Task.CompletedTask;
    }
}