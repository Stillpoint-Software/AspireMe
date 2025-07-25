using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace AspireMe.Infrastructure.Extensions;

public static class LoggerConfigurationExtensions
{
    internal static LoggerConfiguration WithDefaults(this LoggerConfiguration loggerConfiguration, IConfiguration config)
    {
        loggerConfiguration
            .MinimumLevel.Debug()
            .ReadFrom.Configuration(config)
            .Enrich.WithCorrelationId()
            .Enrich.FromLogContext()
            .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Error);

        return loggerConfiguration;
    }

    public static LoggerConfiguration WithFileWriter(this LoggerConfiguration loggerConfiguration, IConfiguration config)
    {
        if (!config.GetValue<bool>("Diagnostics:Targets:File"))
            return loggerConfiguration;

        var appName = config["Api:AppName"] ?? "app";

        var jsonFormatter = new CompactJsonFormatter();
        var pathFormat = $".{Path.DirectorySeparatorChar}logs{Path.DirectorySeparatorChar}{appName}-.json";

        loggerConfiguration.WriteTo.File(jsonFormatter, pathFormat, rollingInterval: RollingInterval.Day, shared: true);
        return loggerConfiguration;
    }
}
