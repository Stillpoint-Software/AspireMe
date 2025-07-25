
#define CONTAINER_DIAGNOSTICS
using FluentValidation;
using Lamar;
using Microsoft.IdentityModel.Logging;
using AspireMe.Api.Commands.SampleArea;
using AspireMe.Api.Endpoints;
using AspireMe.Api.Validators;
using AspireMe.Core.Identity;
using AspireMe.Core.Validators;
using AspireMe.Data.Abstractions.Services;
using AspireMe.Data.PostgreSql;
using AspireMe.Data.PostgreSql.Services;
using AspireMe.Infrastructure.Configuration;
using AspireMe.Infrastructure.Extensions;
using AspireMe.ServiceDefaults;



namespace AspireMe.Api;

public class Startup : IStartupRegistry
{

    public void ConfigureServices(IHostApplicationBuilder builder, IServiceCollection services)
    {
        builder.UseStartup <AspireMe.Infrastructure.Startup > ();

        builder.AddBackgroundServices();
        
        builder.AddNpgsqlDbContext<DatabaseContext>("medical");
        
        

        builder.Configuration
        .AddEnvironmentVariables()
        .AddUserSecrets<Program>(optional: true);
    }

    public void ConfigureApp(WebApplication app, IWebHostEnvironment env)
    {
        app.MapDefaultEndpoints();
        app.MapSampleEndpoints();
        //app.MapControllers();
    }

    public void ConfigureScanner(ServiceRegistry services)
    {
        IdentityModelEventSource.ShowPII = true; // show pii info in logs for debugging openid

        services.Scan(scanner =>
        {
            scanner.AssemblyContainingType<SampleValidation>();
            scanner.ConnectImplementationsToTypesClosing(typeof(IValidator<>));
            scanner.TheCallingAssembly();
            scanner.WithDefaultConventions();
        });

        services.For<ISampleService>().Use<SampleService>();
        services.For<IPrincipalProvider>().Use<PrincipalProvider>();
        services.For<ICreateSampleCommand>().Use<CreateSampleCommand>();
        services.For<IValidatorProvider>().Use<ValidatorProvider>();
    }
    private static void ContainerDiagnostics(IApplicationBuilder app, IHostEnvironment env)
    {
#if CONTAINER_DIAGNOSTICS
        if (!env.IsDevelopment())
            return;

        var container = (IContainer)app.ApplicationServices;
        Console.WriteLine(container.WhatDidIScan());
        Console.WriteLine(container.WhatDoIHave());
#endif
    }
}

public static class StartupExtensions
{
    public static void AddBackgroundServices(this IHostApplicationBuilder builder)
    {
        /* example
        services.Configure<HeartbeatServiceOptions>( x =>
        {
            x.PeriodSeconds = 10;
            x.Text = "ka-thump";
        } );

        services.AddHostedService<HeartbeatService>();       
         */
    }
    
}