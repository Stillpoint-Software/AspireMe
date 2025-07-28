using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace AspireMe.Core.Commands.Middleware;

public interface ITelemetrySourceProvider
{
    ActivitySource GetActivitySource();
}

/// <inheritdoc cref="ITelemetrySourceProvider"/>
public class TelemetrySourceProvider : ITelemetrySourceProvider
{
    private readonly ActivitySource _source;

    public TelemetrySourceProvider( IConfiguration config )
    {
        const string serviceName = "Otter.ServiceBus";
        _source = new ActivitySource( serviceName );
    }

    public ActivitySource GetActivitySource() => _source;
}
