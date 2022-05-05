using k8s;

public class NamespaceTrigger : Triggers
{
    private INamespaceListener k8slistner;
    private List<ActionRunner> actionRunners;
    private string nsName;

    public NamespaceTrigger(string nsName, INamespaceListener k8slistner)
    {
        this.k8slistner = k8slistner;
        this.actionRunners = new List<ActionRunner>();
        this.nsName = nsName;
    }

    public void AddAction(ActionRunner action){
        this.actionRunners.Add(action);
    }

    public List<ActionRunner> GetActions()
    {
        return this.actionRunners;
    }

    public Task StartAsync()
    {
        foreach (var action in this.actionRunners){
            Console.WriteLine($"Registering action for namespace '{this.nsName}'");
            this.k8slistner.RegisterNameSpace(this.nsName, this.RunActionAsync);
        }
        return Task.CompletedTask;
    }

    private async Task RunActionAsync(WatchEventType type)
    {
       Console.WriteLine($"Got event type {type} for namespace {this.nsName}");

        if (type == WatchEventType.Added)
        {
            Console.WriteLine($"starting action for namespace {this.nsName} ...");
            foreach (var action in actionRunners)
            {
                await action.RunAsync(new RunnerConfig { name = this.nsName, type = TriggerType.Namespace.ToString() });
            }
        }

        if (type == WatchEventType.Deleted)
        {
            foreach (var action in actionRunners)
            {
                await action.StopAsync(new RunnerConfig { name = this.nsName, type = TriggerType.Namespace.ToString()  });
            }
        }
    }

}
