var app = WebApplication.CreateBuilder(args)
    .ConfigureApplication(args)
    .Build();

app.ConfigurePipeline();

app.Run();