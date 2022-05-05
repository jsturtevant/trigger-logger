using System.Diagnostics;
using System.Text.Json;
public class WprActionConfig {
    public List<string> profiles { get; set; }
}

public class WprActionRunner : ActionRunner
{
    public List<string> profiles { get; set; }

    public WprActionRunner(JsonElement wprActionConfig)
    {
        var action = JsonSerializer.Deserialize<WprActionConfig>(wprActionConfig);
        profiles = action?.profiles;
    }

    public Task RunAsync(RunnerConfig runnerConfig)
    {
        Console.WriteLine($"Starting WPR action for trigger {runnerConfig.name} ...");

        var profiles = new List<string>();

        foreach (string p in this.profiles)
        {
            Console.WriteLine($"with profile {p} ...");
            profiles.Add($"-start");
            profiles.Add($"{p}");
        }
        Process.Start("wpr", profiles);
        return Task.CompletedTask;
    }

    public Task StopAsync(RunnerConfig config){
        try
        {
            //TODO validate already exists
            Console.WriteLine($"stopping trace for namespace {config.name} ...");
            
            var timeString = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");
            var filename = $"c:\\trace-{config.name}-{timeString}.etl";
            Process.Start("wpr", $"-stop {filename}");
            Console.WriteLine($"file written to {filename}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        return Task.CompletedTask;
    }
}