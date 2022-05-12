using System.Collections.Generic;
using Xunit;
using System.Text.Json;
using System.Threading.Tasks;
using k8s;
using System.IO;
using System.Text.Json.Serialization;

namespace trigger_logger.tests;

public class ConfigurationTests
{
    [Fact]
    public void can_parse_configuration()
    {
        var config = Configuration.ReadFile("config.json");
        Assert.NotNull(config);

        // check that it serializes back properly to same values
        // this will make sure we don't have any errors during parsing
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull

        };
        string output = JsonSerializer.Serialize(config, options);
        Assert.Equal(File.ReadAllText("config.json"), output);
    }

    [Fact]
    public void blank_configuration_should_have_no_triggers()
    {
        var triggers = Configuration.LoadTriggers(new Config(), new fakelistener());
        Assert.Equal<int>(triggers.Count, 0);
    }

    [Fact]
    public void null_configuration_should_have_no_triggers()
    {
        var triggers = Configuration.LoadTriggers(null, new fakelistener());
        Assert.Equal<int>(triggers.Count, 0);
    }

    [Fact]
    public void triggers_should_be_loaded_with_matching_action()
    {
        var config = new Config();
        config.triggers = new List<Trigger>();
        config.triggers.Add(new Trigger()
        {
            name = "test",
            type = TriggerType.Namespace,
            actions = new List<string>() { "test" }
        });
        config.actions = new List<Action>() { new Action() { name = "test", type = "wpr", config = JsonDocument.Parse("{}").RootElement } };


        var triggers = Configuration.LoadTriggers(config, new fakelistener());
        Assert.Equal<int>(triggers.Count, 1);
        var action = triggers[0].GetActions();
        Assert.Equal<int>(action.Count, 1);
        Assert.IsType<WprActionRunner>(action[0]);
    }

    [Fact]
    public void triggers_should_be_loaded_with_matching_actions()
    {
        var config = new Config();
        config.triggers = new List<Trigger>();
        config.triggers.Add(new Trigger()
        {
            name = "test",
            type = TriggerType.Namespace,
            actions = new List<string>() { "test1", "test2" }
        });

        config.actions = new List<Action>();
        config.actions.Add(new Action() { name = "test1", type = "wpr", config = JsonDocument.Parse("{}").RootElement });
        config.actions.Add(new Action() { name = "test2", type = "external", config = JsonDocument.Parse("{}").RootElement });

        var triggers = Configuration.LoadTriggers(config, new fakelistener());
        Assert.Equal<int>(triggers.Count, 1);
        var action = triggers[0].GetActions();
        Assert.Equal<int>(action.Count, 2);
        Assert.IsType<WprActionRunner>(action[0]);
        Assert.IsType<ExternalActionRunner>(action[1]);
    }

    [Fact]
    public void actions_should_be_loaded_with_matching_outputer()
    {
        var config = new Config();
        config.triggers = new List<Trigger>();
        config.triggers.Add(new Trigger()
        {
            name = "test",
            type = TriggerType.Namespace,
            actions = new List<string>() { "test" }
        });
        config.actions = new List<Action>() { new Action() {
                name = "test",
                type = "wpr",
                config = JsonDocument.Parse("{}").RootElement,
                outputs = new List<string>() { "tesoutput" },
            }
        };
        config.outputs = new List<Output>() { new Output() {
                name = "tesoutput",
                type = "az-storage",
                config = JsonDocument.Parse("{}").RootElement 
            }
        };

        var triggers = Configuration.LoadTriggers(config, new fakelistener());
        Assert.Equal<int>(triggers.Count, 1);
        var action = triggers[0].GetActions();
        Assert.Equal<int>(action.Count, 1);
        Assert.IsType<WprActionRunner>(action[0]);

        var outputs = action[0].GetOutputs();
        Assert.Equal(1, outputs.Count);
        Assert.IsType<AzureStorageOutputer>(outputs[0]);
    }

}

public class fakelistener : INamespaceListener
{
    public void RegisterNameSpace(string ns, System.Func<WatchEventType, Task> runner)
    {
        throw new System.NotImplementedException();
    }

    public void Start()
    {
        throw new System.NotImplementedException();
    }
}