using Microsoft.Extensions.Configuration;

using Hyperbee.Migrations.Providers.Postgres;

using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace AspireMe.Migrations.Extensions;

internal static class StartupExtensions
{
    internal static IConfigurationBuilder AddAppSettingsFile(this IConfigurationBuilder builder)
    {
        return builder
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    }

    internal static IConfigurationBuilder AddAppSettingsEnvironmentFile(this IConfigurationBuilder builder)
    {
        return builder
            .AddJsonFile(ConfigurationHelper.EnvironmentAppSettingsName, optional: true);
    }


        }

internal static class ConfigurationHelper
{
    internal static string EnvironmentAppSettingsName => $"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development"}.json";
}
