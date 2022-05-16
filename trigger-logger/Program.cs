using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

public class Program
{
    public static void Main(string[] args)
    {
        var hostBuilder = CreateHostBuilder(args);
        var host = hostBuilder.Build();
        
        var nameSpaceListener = host.Services.GetService<INamespaceListener>();
        nameSpaceListener?.Start();
        host.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
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

            services.Configure<Config>(configurationRoot.GetSection(nameof(Config)));
            services.Configure<kubernetes>(configurationRoot.GetSection(nameof(kubernetes)));

            var configuration = new Config();
            configurationRoot.GetSection(nameof(Config)).Bind(configuration);

            // only load things like Kubernetes ns listener if needed
            if (configuration.triggers.Any(x => x.type == TriggerType.Namespace))
            {
                Console.WriteLine("Creating kubernetes namespace listener");
                services.AddSingleton<INamespaceListener, NamespaceListener>();
            }
        });
}


