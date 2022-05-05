public static class Configuration
{
    public static Dictionary<TriggerType, Triggers> LoadTriggers(Config configuration)
    {
        var triggers = new Dictionary<TriggerType, Triggers>();
        if (configuration == null || configuration.trigger == null)
        {
            return triggers;
        }
        
        foreach (var trigger in configuration.trigger)
        {
            Console.WriteLine($"Processing Trigger '{trigger.name}' of type '{trigger.type}' with action '{trigger.action.type}'");
            switch (trigger.type)
            {
                case TriggerType.Namespace:
                    AddNamespaceTrigger(configuration, triggers, trigger);
                    continue;
                default:
                    Console.WriteLine($"Unknown trigger type '{trigger.type}'");
                    break;
            }
        }

        return triggers;
    }

    public static void AddNamespaceTrigger(Config configuration, Dictionary<TriggerType, Triggers> triggers, Trigger trigger)
    {
        if (triggers.ContainsKey(TriggerType.Namespace))
        {
            var nsTrigger = triggers[TriggerType.Namespace] as NamespaceTrigger;
            nsTrigger.Add(trigger.name, GetActionRunner(trigger.action));
        }
        else
        {
            var nsTrigger = new NamespaceTrigger(configuration.kubeconfig);
            nsTrigger.Add(trigger.name, GetActionRunner(trigger.action));
            triggers.Add(TriggerType.Namespace, nsTrigger);
        }
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

