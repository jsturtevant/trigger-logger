using System.Text.Json;
using System.Text.Json.Serialization;

public class Trigger
{
    public string name { get; set; }
    public TriggerType type { get; set; }
    public List<Action> actions { get; set; }
}
 
public class Action {
    public string type { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public JsonElement config { get; set; }
}

public class Config
{
    public List<Trigger> triggers { get; set; }
    public string kubeconfig {get;set;}
}

public enum TriggerType {
    Namespace
}