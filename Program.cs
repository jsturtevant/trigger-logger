using System.Text.Json;
using System.Text.Json.Serialization;

string configFilePath = "config.json";
if (args.Length > 0)
{
    configFilePath = args[0];
}

// load configuration
Console.WriteLine($"Creating triggers from configuration file '{configFilePath}'");
var configurationString = File.ReadAllText(configFilePath);
Console.WriteLine($"{configurationString}");
var options = new JsonSerializerOptions
{
    WriteIndented = true,
    Converters =
    {
        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
    }
};
Config configuration = JsonSerializer.Deserialize<Config>(configurationString,options);
Dictionary<TriggerType, Triggers> triggers = Configuration.LoadTriggers(configuration);

// Start the triggers and wait for the tasks to complete with Cancellation support
Console.WriteLine($"Starting triggers");
var tasks = new List<Task>();
var cancelation = new CancellationTokenSource();
Console.CancelKeyPress += (sender, eventArgs) => cancelation.Cancel();
foreach (var trigger in triggers)
{
    tasks.Add(trigger.Value.StartAsync());
}
Task.WaitAll(tasks.ToArray(), cancelation.Token);
Console.WriteLine($"Exiting program");
