using Serilog;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
builder.Logging.AddSerilog(logger);

builder.Services.AddMyData(builder.Configuration);

builder.Services.AddMySwagger();

builder.Services.AddMyAuthentication(builder.Configuration);

var app = builder.Build();

app.UseHttpLogging();

app.RegisterItemApis();

app.RegisterUserApis(builder.Configuration);

app.UseMySwagger();

app.UseMyAuthentication();

app.MapGet("/", () => "Hello from Minimal API");

app.Run();