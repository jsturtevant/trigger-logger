using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace trigger_logger.tests;

public class ConfigLoadingTests
{
    private IHostBuilder _hostBuilder;
    
    public ConfigLoadingTests()
    {
        _hostBuilder = Program.CreateHostBuilder(null);
    }
    
    [Fact]
    public async Task ShouldLoadConfigFile()
    {
        _hostBuilder.ConfigureAppConfiguration(builder =>
        {
            builder.Sources.Clear();
            builder.AddJsonFile("test-config.json");
        });
        var host = _hostBuilder.Build();

        var opts = host.Services.GetService<IOptions<Config>>().Value;
        Assert.Equal(4, opts.triggers.Count);
        Assert.Equal(4, opts.actions.Count);
        Assert.Equal(1, opts.outputs.Count);

    }

    [Fact]
    public async Task ShouldOnlyLoadListenerWithTriggers()
    {

        _hostBuilder.ConfigureAppConfiguration(builder =>
        {
            builder.Sources.Clear();
            builder.AddJsonFile("no-triggers.json");
        });

        var host = _hostBuilder.Build();

        var listener = host.Services.GetService<INamespaceListener>();
        Assert.Null(listener);
    }
}