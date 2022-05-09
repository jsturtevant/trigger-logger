public interface ActionRunner {
    public Task RunAsync(RunnerConfig config);
    public Task StopAsync(RunnerConfig config);

    public List<Outputers> GetOutputs();
    public void AddOutput(Outputers output);
}

public class RunnerConfig{
    public string name;
    public string type;
}
