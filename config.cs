using System.Text.Json;
using System.Text.Json.Serialization;

public class Trigger
{
    public string name { get; set; }
    public TriggerType type { get; set; }
    public Action action { get; set; }
}
 
public class Action {
    public string type { get; set; }

    public JsonElement config { get; set; }
}

public class Config
{
    public List<Trigger> trigger { get; set; }
    public string kubeconfig {get;set;}
}

public enum TriggerType {
    Namespace
}