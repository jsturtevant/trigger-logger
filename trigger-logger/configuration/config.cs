using System.Text.Json;
using System.Text.Json.Serialization;

public class Trigger
{
    public string name { get; set; }
    public TriggerType type { get; set; }
    public List<string> actions { get; set; }
}
 
public class Action {
    public string name { get; set; }
    public string type { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public JsonElement config { get; set; }
    public List<string> outputs { get; set; }
}

public class Output {
    public string name { get; set; }
    public string type { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public JsonElement config { get; set; }
}

public class Config
{
    public List<Trigger> triggers { get; set; }
     public List<Action> actions { get; set; }

     public List<Output> outputs { get; set; }
}

public class kubernetes {
    public string kubeconfig {get;set;}
}

public enum TriggerType {
    Namespace
}