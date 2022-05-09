using System.Diagnostics;
using System.Text.Json;
public class WprActionConfig {
    public List<string> profiles { get; set; }
}

public class WprActionRunner : ActionRunner
{
    private List<string> profiles { get; set; }
    private List<Outputers> outputs { get; set; }

    public WprActionRunner(JsonElement wprActionConfig)
    {
        var action = JsonSerializer.Deserialize<WprActionConfig>(wprActionConfig);
        profiles = action?.profiles;
        this.outputs = new List<Outputers>();
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

    public async Task StopAsync(RunnerConfig config){
        try
        {
            //TODO validate already exists
            Console.WriteLine($"stopping trace for namespace {config.name} ...");
            
            var timeString = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");
            var filename = $"c:\\trace-{config.name}-{timeString}.etl";
            Process.Start("wpr", $"-stop {filename}");
            Console.WriteLine($"file written to {filename}");

            if (outputs.Count > 0)
            {
                Console.WriteLine($"external outputs are registered");
                await WaitForFile(filename);
                foreach (var output in outputs)
                {
                    await output.Run(filename);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    // WaitForFile will wait till file is created and locks released
    // it is possible that something would take a lock on this file after it is created
    // but before we can read it but unlikely in our scenario
    private static async Task WaitForFile(string filename)
    {
        var sw = new Stopwatch();
        sw.Start();

        var waiting = true;
        while (waiting)
        {
            Console.WriteLine($"waiting for file {filename} to be written...");
            if (sw.ElapsedMilliseconds > 10000)
            {
                Console.WriteLine($"file {filename} not found");
            }

            FileStream fs = null;
            try {
                fs = new FileStream (filename, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                waiting = false;
                Console.WriteLine($"File {filename} to be found...");
            }
            catch (IOException) {
                if (fs != null) {
                    fs.Dispose ();
                }
                await Task.Delay(1000);
            }
            finally {
                if (fs != null) {
                    fs.Dispose();
                }
            }
        }
    }

    public List<Outputers> GetOutputs()
    {
        return this.outputs;
    }

    public void AddOutput(Outputers output)
    {
        this.outputs.Add(output);
    }
}