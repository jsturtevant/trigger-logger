using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

string configFilePath = "config.json";
if (args.Length > 0)
{
    configFilePath = args[0];
}

// load configuration
var configurationString = File.ReadAllText(configFilePath);
Console.WriteLine($"{configurationString}");

Console.WriteLine($"Creating triggers from configuration file '{configFilePath}'");

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

// Start the triggers and wait for the tasks to complete with cancelation support
var tasks = new List<Task>();
var cancelation = new CancellationTokenSource();
Console.CancelKeyPress += (sender, eventArgs) => cancelation.Cancel();

Console.WriteLine($"Starting triggers");
foreach (var trigger in triggers)
{
    tasks.Add(trigger.Value.StartAsync());
}
Task.WaitAll(tasks.ToArray(), cancelation.Token);
Console.WriteLine($"Exiting program");