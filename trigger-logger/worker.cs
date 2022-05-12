
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly Config config;
    private readonly INamespaceListener namespaceListener;

    public Worker(IOptions<Config> config,
                  ILogger<Worker> logger, 
                  INamespaceListener namespaceListener)
    {
        _logger = logger;
        this.config = config.Value;
        this.namespaceListener = namespaceListener;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        List<Triggers> triggers = Configuration.LoadTriggers(this.config, namespaceListener);

        // Start the triggers and wait for the tasks to complete with Cancellation support
        _logger.LogInformation($"Starting triggers");
        var tasks = new List<Task>();
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
