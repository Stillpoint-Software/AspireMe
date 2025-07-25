
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Hyperbee.Pipeline.Context;
using Lamar.Microsoft.DependencyInjection;
using AspireMe.Core.Identity;
using AspireMe.Core.Validators;
using AspireMe.Infrastructure.Data;
using AspireMe.Infrastructure.IoC;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Audit.Core;

using AspireMe.Data.Abstractions.Services;
using AspireMe.Data.PostgreSql.Services;

namespace AspireMe.Infrastructure.Configuration;

public static class LamarSetup
{
    public static void ConfigureLamar(IHostBuilder builder)
    {
        builder.UseLamar(registry =>
        {
            registry.Scan(scan =>
            {
                scan.AssemblyContainingType<RegisterServiceAttribute>();

                scan.WithDefaultConventions();
                scan.ConnectImplementationsToTypesClosing(typeof(IValidator<>));
                scan.Convention<RegisterServiceConvention>();
            });

            // Manual helper registrations
            registry.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            registry.AddSingleton<IPrincipalProvider, PrincipalProvider>();
            registry.AddSingleton<IPipelineContextFactory, PipelineContextFactory>();
            registry.AddSingleton<IValidatorProvider, ValidatorProvider>();
            registry.AddSingleton<ISampleService, SampleService>();

            // MVC + JSON settings
            registry.AddControllers()
                    .AddJsonOptions(opts =>
                    {
                        opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                        opts.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                        opts.JsonSerializerOptions.Converters.Add(new JsonBoolConverter());
                        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    });
        });
    }
}
