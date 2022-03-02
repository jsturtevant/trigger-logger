
public class Action
{
    public string type { get; set; }
    public List<string> profiles { get; set; }
}

public class Trigger
{
    public string name { get; set; }
    public string type { get; set; }
    public Action Action { get; set; }
}

public class Config
{
    public List<Trigger> trigger { get; set; }
    public string kubeconfig {get;set;}
}
