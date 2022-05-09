using System.Text.Json;
using System.Text.Json.Serialization;
public static class Configuration
{

    public static Config ReadFile(string configFilePath)
    {
        Console.WriteLine($"Creating triggers from configuration file '{configFilePath}'");
        string configurationString = File.ReadAllText(configFilePath);
        Console.WriteLine($"{configurationString}");
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        
        };
        return JsonSerializer.Deserialize<Config>(configurationString, options);
    }

    public static List<Triggers> LoadTriggers(Config configuration, INamespaceListener k8slistner)
    {
        var triggers = new List<Triggers>();
        if (configuration == null || configuration.triggers == null)
        {
            return triggers;
        }
        
        foreach (var trigger in configuration.triggers)
        {
            Console.WriteLine($"Processing configuration for trigger '{trigger.name}' of type '{trigger.type}");
            switch (trigger.type)
            {
                case TriggerType.Namespace:
                    var nsTrigger = new NamespaceTrigger(trigger.name, k8slistner);

                    foreach (var actionName in trigger.actions)
                    {
                        Console.WriteLine($"Processing action configuration '{actionName}' for trigger '{trigger.name}' of type '{trigger.type}" );
                        var action = configuration.actions.First(x => x.name == actionName);
                        var actionRunner = GetActionRunner(action);

                        if (action.outputs != null && action.outputs.Count > 0){
                            foreach (var outputName in action.outputs)
                            {
                                var output = configuration.outputs.First(x => x.name == action.outputs[0]);
                                actionRunner.AddOutput(GetOutputter(output));
                            }
                        }
                        nsTrigger.AddAction(actionRunner);
                    }

                    triggers.Add(nsTrigger);
                    break;
                default:
                    Console.WriteLine($"Unknown trigger type '{trigger.type}'");
                    break;
            }
        }

        return triggers;
    }

    public static ActionRunner GetActionRunner(Action action)
    {
        switch (action.type)
        {
            case "wpr":
                return new WprActionRunner(action.config);
            case "external":
                return new ExternalActionRunner(action.config);
            case "console":
                return new ConsoleActionRunner();
            default:
                Console.WriteLine($"Unknown action type '{action.type}'");
                return null;
        }
    } 

    public static Outputers GetOutputter(Output output)
    {
        switch (output.type)
        {
            case "az-storage":
                return new AzureStorageOutputer(output.config);
            default:
                Console.WriteLine($"Unknown action type '{output.type}'");
                return null;
        }
    } 
}

