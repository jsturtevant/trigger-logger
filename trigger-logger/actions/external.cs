using System.IO.Compression;
using System.Text.Json;
using System.Diagnostics;

public class ExternalActionConfig {
    public string expand { get; set; }
    public string addToPath { get; set; }
    public string url { get; set; }
    public List<string> startCommand { get; set; }
    public List<string> stopCommand { get; set; }
    public string downloadLocation {get;set;}
    public string downloadFileName {get;set;}
}

public class ExternalActionRunner :  ActionRunnerBase, ActionRunner
{
    static readonly HttpClient client = new HttpClient();
    private ExternalActionConfig? config;

    public ExternalActionRunner(JsonElement wprAction)
    {
        this.config = JsonSerializer.Deserialize<ExternalActionConfig>(wprAction);
    }

    // to do make this more robust
    public async Task RunAsync(RunnerConfig config)
    {
        Console.WriteLine($"Starting action 'external' for trigger {config.name} ...");

       
        byte[] fileBytes = await client.GetByteArrayAsync(this.config.url);
        Directory.CreateDirectory(this.config.downloadLocation);

        string filePath = Path.Combine(this.config.downloadLocation, this.config.downloadFileName);
        File.WriteAllBytes(filePath, fileBytes);

        if (this.config.expand == "zip")
        {
            Console.WriteLine($"unzipping to path {this.config.downloadLocation}");
            ZipFile.ExtractToDirectory(filePath, this.config.downloadLocation, true);
        }

        if (this.config.addToPath != null)
        {
            Console.WriteLine($"adding {this.config.downloadLocation} to PATH");
            Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + this.config.downloadLocation);
        }

        var timeString = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");
        var filename = $"c:\\trace-{config.name}-{timeString}.etl";

        try{
            var originalArgs = string.Join(" ", this.config.startCommand.GetRange(1, this.config.startCommand.Count - 1));
            var args = originalArgs.Replace("{tracefile}", filename);
            Process.Start(this.config.startCommand.FirstOrDefault(), args);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public Task StopAsync(RunnerConfig config)
    {
        Console.WriteLine($"stopping action 'external' for trigger {config.name} ...");
        try{
            var args = string.Join(" ", this.config.stopCommand.GetRange(1, this.config.stopCommand.Count - 1));
            Process.Start(this.config.stopCommand.First(), args);

            //todo :  await RunoutputsAsync(filename);
        }
        catch (Exception e)
        {
            Console.WriteLine($"error stopping action 'external' for trigger {config.name} ...");
            Console.WriteLine(e.Message);
        }
        return Task.CompletedTask;
    }
}