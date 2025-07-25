
using AspireMe.ServiceDefaults;
using AspireMe.Infrastructure.Extensions;
using Serilog;

namespace AspireMe.Api;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.AddServiceDefaults();

            Log.Information("Starting host...");
            Log.Information("Using environment settings '{EnvironmentAppSettingsName}'.", ConfigurationHelper.EnvironmentAppSettingsName);

            var app = builder.ConfigureApplication(configure =>
            {
                configure.UseStartup<Startup>();
            });

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unhandled exception during startup");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }

        return 0;
    }
}