

string configFilePath = "config.json";
if (args.Length > 0)
{
    configFilePath = args[0];
}

// load configuration
Config configuration = Configuration.ReadFile(configFilePath);

var tasks = new List<Task>();
INamespaceListener listener = null;
if (configuration.triggers.Any(x => x.type == TriggerType.Namespace))
{
    Console.WriteLine("Creating kubernetes namespace listener");
    listener = new NamespaceListener(configuration.kubeconfig);
    tasks.Add(listener.Run());
}

List<Triggers> triggers = Configuration.LoadTriggers(configuration, listener);

// Start the triggers and wait for the tasks to complete with Cancellation support
Console.WriteLine($"Starting triggers");

var cancelation = new CancellationTokenSource();
Console.CancelKeyPress += (sender, eventArgs) => cancelation.Cancel();
foreach (var trigger in triggers)
{
    tasks.Add(trigger.StartAsync());
}
Task.WaitAll(tasks.ToArray(), cancelation.Token);
Console.WriteLine($"Exiting program");
