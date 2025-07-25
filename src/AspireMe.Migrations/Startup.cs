using Hyperbee.Migrations;
using AspireMe.ServiceDefaults;

namespace AspireMe.Migrations;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    public void ConfigureServices(IServiceCollection services)
    {

        services.AddCors(c => c.AddPolicy("CorsAllowAll", build =>
        {
            build.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        }));


        services.AddHttpContextAccessor();

        services.AddHttpClient();
    }

    public void Configure(WebApplication app, IWebHostEnvironment env)
    {
        // Use appropriate middleware based on the environment
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHsts();
        }

        // General middleware setup
        app.UseRouting();
        app.UseCors(c => // must be called before UseResponseCaching
        {
            c.AllowAnyOrigin();
            c.AllowAnyMethod();
            c.AllowAnyHeader();
        });

        app.MapDefaultEndpoints();

        var migrationService = app.Services.GetRequiredService<MigrationRunner>();
        migrationService.RunAsync().GetAwaiter().GetResult();
    }
}
