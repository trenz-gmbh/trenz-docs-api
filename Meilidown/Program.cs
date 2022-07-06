using Meilidown;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => { services.AddHostedService<Worker>(); })
    .ConfigureAppConfiguration(c =>
    {
        c.AddJsonFile("appsettings.local.json", optional: true);
    })
    .Build();

await host.RunAsync();