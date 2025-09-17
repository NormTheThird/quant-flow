var builder = WebApplication.CreateBuilder(args);
builder.ConfigureServices();
builder.ConfigureAppConfiguration(args);

var app = builder.Build();
app.ConfigurePipeline();

app.Run();