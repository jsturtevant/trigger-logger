using k8s;
using k8s.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public interface INamespaceListener
{
    void RegisterNameSpace(string ns,Func<WatchEventType, Task> runner);
    public void Start();
}

public class NamespaceListener : INamespaceListener
{
    private Kubernetes client;
    private CancellationToken cancelationToken;
    private ILogger<NamespaceListener> logger;

    private Dictionary<string, List<Func<WatchEventType, Task>>> ns { get; set; }

    public NamespaceListener(IOptions<kubernetes> kubernetesConfig, 
                             ILogger<NamespaceListener> logger,
                             IHostApplicationLifetime applicationLifetime)
    {
        this.logger = logger;
        this.ns = new Dictionary<string, List<Func<WatchEventType, Task>>>();
        
        var k8sConfig = KubernetesClientConfiguration.BuildConfigFromConfigFile(kubernetesConfig.Value.kubeconfig);
        client = new Kubernetes(k8sConfig);
        this.cancelationToken = applicationLifetime.ApplicationStopping;
    }

    public void RegisterNameSpace(string ns, Func<WatchEventType, Task> action)
    {
        if (!this.ns.ContainsKey(ns))
        {
            this.ns.Add(ns, new List<Func<WatchEventType, Task>>());
        }
        this.ns[ns].Add(action);
        this.logger.LogInformation("Registered action for namespace " + ns);
    }

    public void Start()
    {
        Task.Run(async () => await StartAsync());
    }

    private async Task StartAsync(){
         var nsResp = client.ListNamespaceWithHttpMessagesAsync(watch: true, cancellationToken: this.cancelationToken);

        this.logger.LogInformation("Waiting for namespace events");
        await foreach (var (type, ns) in nsResp.WatchAsync<V1Namespace, V1NamespaceList>())
        {
            this.logger.LogInformation($"{type} event for namespace {ns.Metadata.Name}");
            if (this.ns.ContainsKey(ns.Metadata.Name))
            {
                // this is fire and forget of these actions
                this.logger.LogInformation($"Firing events for namespace {ns.Metadata.Name}");
                this.ns[ns.Metadata.Name].ForEach(x => x(type));
            }
        }
    }
}


