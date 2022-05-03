using k8s;
using k8s.Models;
public class NamespaceTrigger : Triggers
{
    private readonly string configString;
    private Dictionary<string, List<ActionRunner>> nsActions = new Dictionary<string, List<ActionRunner>>();

    public NamespaceTrigger(string configString)
    {
        this.nsActions = new Dictionary<string, List<ActionRunner>>();
        this.configString = configString;
    }

    public void Add(string ns, ActionRunner action){
        // if not present add to dictionary 
        if (!this.nsActions.ContainsKey(ns))
        {
            this.nsActions.Add(ns, new List<ActionRunner>());
        }

        var actions = this.nsActions[ns];
        actions.Add(action);

    }

    public async Task StartAsync()
    {
        var k8sConfig = KubernetesClientConfiguration.BuildConfigFromConfigFile(this.configString);
        var client = new Kubernetes(k8sConfig);

        var nsResp = client.ListNamespaceWithHttpMessagesAsync(watch: true);

        Console.WriteLine("Waiting for namespace events");
        await foreach (var (type, ns) in nsResp.WatchAsync<V1Namespace, V1NamespaceList>())
        {
            Console.WriteLine(type);
            Console.WriteLine(ns.Metadata.Name);

            if (this.nsActions.ContainsKey(ns.Metadata.Name) && type.ToString() == "Added")
            {
                Console.WriteLine($"starting action for namespace {ns.Metadata.Name} ...");
                var actions = this.nsActions[ns.Metadata.Name];
                foreach (var action in actions)
                {
                    await action.RunAsync(new RunnerConfig { name = ns.Metadata.Name });
                }
            }

            if (this.nsActions.ContainsKey(ns.Metadata.Name) && type.ToString() == "Deleted")
            {
                var actions = this.nsActions[ns.Metadata.Name];
                foreach (var action in actions)
                {
                    await action.StopAsync(new RunnerConfig { name = ns.Metadata.Name });
                }
            }
        }
        Console.WriteLine("Completed waiting for namespace events");
    }
}


