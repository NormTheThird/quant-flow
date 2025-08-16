var host = Host.CreateDefaultBuilder(args)
    .ConfigureApplication(args)
    .Build();

await host.RunAsync();