var hostBuilder = Host.CreateDefaultBuilder(args)
    .ConfigureApplication(args)
    .Build();

await hostBuilder.RunAsync();