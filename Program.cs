using System.Diagnostics;
using System.Text.Json;
using k8s;
using k8s.Models;

string configFilePath = "config.json";
if (args.Length > 0)
{
    configFilePath = args[0];
}
var configurationString = File.ReadAllText(configFilePath);
Console.WriteLine($"{configurationString}");

Config configuration = JsonSerializer.Deserialize<Config>(configurationString);

// TODO handle more configuration as well as use a pattern
var nsAction = configuration.trigger.Where(x => x.type == "Namespace")?.First();

var k8sConfig = KubernetesClientConfiguration.BuildConfigFromConfigFile(configuration.kubeconfig);
var client = new Kubernetes(k8sConfig);

var podlistResp = client.ListNamespacedPodWithHttpMessagesAsync("default", watch: true);
var nsResp = client.ListNamespaceWithHttpMessagesAsync(watch: true);

await foreach (var (type, item) in nsResp.WatchAsync<V1Namespace, V1NamespaceList>())
{
    Console.WriteLine(type);
    Console.WriteLine(item.Metadata.Name);

    if (item.Metadata.Name == nsAction.name && type.ToString() == "Added")
    {
        try
        {
            //TODO validate not already started
            Console.WriteLine($"starting trace for namespace {nsAction.name} ...");
            var profiles = new List<string>();
            foreach(string p in nsAction.Action.profiles){
                Console.WriteLine($"with profile {p} ...");
                profiles.Add($"-start");
                profiles.Add($"{p}");
            }
            Process.Start("wpr", profiles);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    if (item.Metadata.Name == nsAction.name && type.ToString() == "Deleted")
    {
        try
        {
            //TODO validate already exists
            Console.WriteLine($"stopping trace for namespace {nsAction.name} ...");
            
            var timeString = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");
            var filename = $"c:\\trace-{nsAction.name}-{timeString}.etl";
            Process.Start("wpr", $"-stop {filename}");
            Console.WriteLine($"file written to {filename}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}

