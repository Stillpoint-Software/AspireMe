using Hyperbee.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AspireMe.Migrations;

public class MainService : BackgroundService
{
    //private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly ILogger<MainService> _logger;
    private readonly IServiceProvider _serviceProvider;
    public const string ActivitySourceName = "Migrations";
    private readonly ActivitySource _activitySource = new(ActivitySourceName);

    public MainService(IServiceProvider serviceProvider, ILogger<MainService> logger)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var provider = scope.ServiceProvider;

        await Task.Yield(); // yield to allow startup logs to write to console

        using var activity = _activitySource.StartActivity("Running Migrations", ActivityKind.Client);

        try
        {
            var runner = provider.GetRequiredService<MigrationRunner>();
            await runner.RunAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Migrations encountered an unhandled exception.");
        }

        // _applicationLifetime.StopApplication();
    }
}
