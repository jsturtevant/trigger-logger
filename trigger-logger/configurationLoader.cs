public static class Configuration
{
    public static List<Triggers> LoadTriggers(Config configuration, INamespaceListener k8slistner)
    {
        var triggers = new List<Triggers>();
        if (configuration == null || configuration.trigger == null)
        {
            return triggers;
        }
        
        foreach (var trigger in configuration.trigger)
        {
            Console.WriteLine($"Processing configuration for trigger '{trigger.name}' of type '{trigger.type}' with action '{trigger.action.type}'");
            switch (trigger.type)
            {
                case TriggerType.Namespace:
                    var nsTrigger = new NamespaceTrigger(trigger.name, k8slistner);
                    nsTrigger.AddAction(GetActionRunner(trigger.action));
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
            default:
                Console.WriteLine($"Unknown action type '{action.type}'");
                return null;
        }
    } 
}

