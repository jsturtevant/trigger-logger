using k8s;
using k8s.Models;
public interface INamespaceListener
{
    void RegisterNameSpace(string ns,Func<WatchEventType, Task> runner);
    Task Run();
}

public class NamespaceListener : INamespaceListener
{
    private Kubernetes client;

    private Dictionary<string, List<Func<WatchEventType, Task>>> ns { get; set; }

    public NamespaceListener(string configString)
    {
        this.ns = new Dictionary<string, List<Func<WatchEventType, Task>>>();
        var k8sConfig = KubernetesClientConfiguration.BuildConfigFromConfigFile(configString);
        client = new Kubernetes(k8sConfig);
    }

    public async Task Run()
    {
        var nsResp = client.ListNamespaceWithHttpMessagesAsync(watch: true);

        Console.WriteLine("Waiting for namespace events");
        await foreach (var (type, ns) in nsResp.WatchAsync<V1Namespace, V1NamespaceList>())
        {
            Console.WriteLine($"{type} event for namespace {ns.Metadata.Name}");
            if (this.ns.ContainsKey(ns.Metadata.Name))
            {
                // this is fire and forget of these actions
                Console.WriteLine($"Firing events for namespace {ns.Metadata.Name}");
                this.ns[ns.Metadata.Name].ForEach(x => x(type));
            }
        }
    }

    public void RegisterNameSpace(string ns, Func<WatchEventType, Task> action)
    {
        if (!this.ns.ContainsKey(ns))
        {
            this.ns.Add(ns, new List<Func<WatchEventType, Task>>());
        }
        this.ns[ns].Add(action);
        Console.WriteLine("Registered action for namespace " + ns);
    }
}


