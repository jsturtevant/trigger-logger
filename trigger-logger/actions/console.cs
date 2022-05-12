public class ConsoleActionRunner : ActionRunnerBase, ActionRunner
{
    public Task RunAsync(RunnerConfig config)
    {
        Console.Write($"Start console action for trigger {config.name} of type {config.type}");
        return Task.CompletedTask;
    }

    public Task StopAsync(RunnerConfig config)
    {
        Console.Write($"Stop console action for trigger {config.name} of type {config.type}");
        return Task.CompletedTask;
    }
}