using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, configuration) =>
    {
        configuration.Sources.Clear();

        IHostEnvironment env = hostingContext.HostingEnvironment;

        configuration
            .AddJsonFile("config.json", optional: false, reloadOnChange: false);
    })
    .ConfigureServices((hostContext, services) =>
    {
        var configurationRoot = hostContext.Configuration;
        services.AddHostedService<Worker>();

        // hack as configuration service doesn't really handle the multiple type configuration being used.
        // Maybe there is a better way to do this? or not use this at all?
        Config configuration = Configuration.ReadFile("config.json");
        services.Configure<Config>((c) => {
            c.actions = configuration.actions; 
            c.triggers = configuration.triggers; 
            c.outputs = configuration.outputs;}
        );

        services.Configure<kubernetes>(configurationRoot.GetSection(nameof(kubernetes)));

        // only load things like Kubernetes ns listener if needed
        if (configuration.triggers.Any(x => x.type == TriggerType.Namespace))
        {
            Console.WriteLine("Creating kubernetes namespace listener");
            services.AddSingleton<INamespaceListener, NamespaceListener>();
        }
    })
    .Build();

var nameSpaceListener = host.Services.GetService<INamespaceListener>();
if (nameSpaceListener is not null){
    nameSpaceListener.Start();
}
await host.RunAsync();
