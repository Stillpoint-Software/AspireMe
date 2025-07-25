using System.Text.Json;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Hyperbee.Pipeline;
using Lamar;
using AspireMe.Core.Validators;
using AspireMe.Infrastructure.Configuration;
using AspireMe.Infrastructure.Data;
using AspireMe.Infrastructure.IoC;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;


namespace AspireMe.Infrastructure;

public class Startup : IStartupRegistry
{
    public void ConfigureServices(IHostApplicationBuilder builder, IServiceCollection services)
    {
        services.AddCors(c => c.AddPolicy("CorsAllowAll", build =>
        {
            build.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        }));

        services.AddHttpContextAccessor();
        services.AddHttpClient();

        services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = ApiVersion.Default;
            options.ApiVersionReader = new HeaderApiVersionReader("X-Version");
        });

        services.AddControllers()
        .AddJsonOptions(x =>
        {
            // serialize enums as strings in api responses (e.g. Color)
            x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            x.JsonSerializerOptions.Converters.Add(new JsonBoolConverter());
            x.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            x.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

        services.AddDataProtection();

        //Authorization and Authentication
        

        //Azure Blob Storage and Key Vault
        
        

        // Add Pipeline and Proxy Service
        services.AddPipeline((factoryServices, rootProvider) =>
        {
            factoryServices.ProxyService<IValidatorProvider>(rootProvider);
        });

        // Add Swagger for API documentation
        services.AddEndpointsApiExplorer(); //AV this has to go first
        services.AddSwaggerGen();

        // Add services to the container before calling Build
        builder.Services.AddProblemDetails();

        // Configure Serilog setup
        SerilogSetup.ConfigureSerilog(builder);

        
        // Configure audit setup
        AuditSetup.ConfigureAudit(builder);
        
    }

    public void ConfigureApp(WebApplication app, IWebHostEnvironment env)
    {
        // Use appropriate middleware based on the environment
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
           {
               c.SwaggerEndpoint("/swagger/v1/swagger.json", "PAspire API v1");
               c.RoutePrefix = string.Empty;  // Makes Swagger UI available at the root ("/")

           });
        }
        else
        {
            app.UseHsts();
            // General middleware setup
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors(c => // must be called before UseResponseCaching
            {
                c.AllowAnyOrigin();
                c.AllowAnyMethod();
                c.AllowAnyHeader();
            });

            app.UseAuthorization();
        }
    }

    public void ConfigureScanner(ServiceRegistry services)
    {
        IdentityModelEventSource.ShowPII = true; // show pii info in logs for debugging openid

        services.Scan(scanner =>
        {
            scanner.TheCallingAssembly();
            scanner.WithDefaultConventions();
            scanner.WithRegisterServiceConventions();
        });
    }
}