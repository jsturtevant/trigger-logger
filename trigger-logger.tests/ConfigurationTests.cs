using System.Collections.Generic;
using Xunit;
using System.Text.Json;
using System.Threading.Tasks;
using k8s;

namespace trigger_logger.tests;

public class ConfigurationTests
{
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


        var triggers = Configuration.LoadTriggers(config, new fakelistener());
        Assert.Equal<int>(triggers.Count, 1);
        var action = triggers[0].GetActions();
        Assert.Equal<int>(action.Count, 1);
        Assert.IsType<WprActionRunner>(action[0]);
    }

}

public class fakelistener : INamespaceListener
{
    public void RegisterNameSpace(string ns, System.Func<WatchEventType, Task> runner)
    {
        throw new System.NotImplementedException();
    }

    public Task Run()
    {
        throw new System.NotImplementedException();
    }
}