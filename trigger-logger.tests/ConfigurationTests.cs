using System.Collections.Generic;
using Xunit;
using System.Text.Json;

namespace trigger_logger.tests;

public class ConfigurationTests
{
    [Fact]
    public void blank_configuration_should_have_no_triggers()
    {
        var triggers = Configuration.LoadTriggers(new Config());
        Assert.Equal<int>(triggers.Count, 0);
    }

    [Fact]
    public void null_configuration_should_have_no_triggers()
    {
        var triggers = Configuration.LoadTriggers(null);
        Assert.Equal<int>(triggers.Count, 0);
    }

    [Fact]
    public void triggers_should_be_loaded_with_matching_actions()
    {

        const string wprConfig = "{ \"profiles\": [ \"test\", \"test2\" ]}";
        var config = new Config();
        config.trigger = new List<Trigger>();
        config.trigger.Add(new Trigger()
        {
            name = "test",
            type = TriggerType.Namespace,
            action = new Action() { type = "wpr", config = JsonDocument.Parse(wprConfig).RootElement }
        });


        var triggers = Configuration.LoadTriggers(config);
        Assert.Equal<int>(triggers.Count, 1);
        var action = triggers[TriggerType.Namespace].GetActions();
        Assert.Equal<int>(action.Count, 1);
        Assert.IsType<WprActionRunner>(action[0]);
    }

}