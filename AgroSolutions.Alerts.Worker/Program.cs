using AgroSolutions.Alerts.Application.Interfaces;
using AgroSolutions.Alerts.Application.Services;
using AgroSolutions.Alerts.Domain.Interfaces;
using AgroSolutions.Alerts.Infrastructure.Adapters;
using AgroSolutions.Alerts.Infrastructure.Data;
using AgroSolutions.Alerts.Infrastructure.Messaging;
using AgroSolutions.Alerts.Infrastructure.Repositories;
using AgroSolutions.Alerts.Infrastructure.Services;
using AgroSolutions.Alerts.Worker.Workers;
using Amazon.SQS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;

var builder = Host.CreateApplicationBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AgroContext>(options =>
{
    options.UseNpgsql(connectionString);
}, ServiceLifetime.Scoped); 

builder.Services.AddTransient<ITelemetryParser, TelemetryJsonParser>();
builder.Services.AddTransient<INotificationService, ConsoleNotificationService>();
builder.Services.AddTransient<ITelemetryProcessingService, TelemetryProcessingService>();

var awsOptions = builder.Configuration.GetAWSOptions();
builder.Services.AddDefaultAWSOptions(awsOptions);
builder.Services.AddAWSService<IAmazonSQS>();


builder.Services.AddScoped<ITelemetryRepository, AlertRepository>();
builder.Services.AddSingleton<IMessageConsumer, AwsSqsConsumer>();

builder.Services.AddHttpClient<IHistoryIntegrationService, HistoryIntegrationService>(client =>
{
    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("HistoryApiUrl") ?? "http://localhost:5000");
})
.AddPolicyHandler(GetRetryPolicy());
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AgroContext>();
    db.Database.Migrate();
}

host.Run();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}