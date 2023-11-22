using System.Reflection;

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

        if (data.ActivePowerOutlet > 0.008m && data.ActivePowerOutlet != 0m)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Elmätare: {id}]");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("ActivePowerOutlet  : " + data.ActivePowerOutlet);
            Console.WriteLine("ActivePowerOutlet 1: " + data.ActivePowerOutlet1);
            Console.WriteLine("ActivePowerOutlet 2: " + data.ActivePowerOutlet2);
            Console.WriteLine("ActivePowerOutlet 3: " + data.ActivePowerOutlet3);
            Console.WriteLine(DateTime.Now);

            Console.WriteLine();
            Console.WriteLine();
        }


        //foreach (PropertyInfo property in typeof(RawEnergyMeterData).GetProperties())
        //{

        //    var value = property.GetValue(data, null);

        //    // Check if the value is not null and not 0
        //    if (value != null && !value.Equals(decimal.Zero))
        //    {
        //        Console.WriteLine($"{property.Name}: {value}");
        //    }

        //}



        return Task.CompletedTask;
    }
}