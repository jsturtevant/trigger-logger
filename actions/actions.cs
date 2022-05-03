public interface ActionRunner {
    public Task RunAsync(RunnerConfig config);
    public Task StopAsync(RunnerConfig config);
}

public class RunnerConfig{
    public string name;
    public string type;
}
