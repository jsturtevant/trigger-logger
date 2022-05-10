
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string configFilePath = "config.json";

        // load configuration
        Config configuration = Configuration.ReadFile(configFilePath);

        var tasks = new List<Task>();
        INamespaceListener listener = null;
        if (configuration.triggers.Any(x => x.type == TriggerType.Namespace))
        {
            _logger.LogInformation("Creating kubernetes namespace listener");
            listener = new NamespaceListener(configuration.kubeconfig);
            tasks.Add(listener.Run());
        }

        List<Triggers> triggers = Configuration.LoadTriggers(configuration, listener);

        // Start the triggers and wait for the tasks to complete with Cancellation support
        _logger.LogInformation($"Starting triggers");
        foreach (var trigger in triggers)
        {
            tasks.Add(trigger.StartAsync());
        }

        try {
            Task.WaitAll(tasks.ToArray(), stoppingToken);
        } catch(OperationCanceledException){
            _logger.LogDebug("Tasks were canceled during shutdown");
        }
    }
}
