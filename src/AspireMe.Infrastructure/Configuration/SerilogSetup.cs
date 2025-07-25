using Microsoft.Extensions.Hosting;
using Serilog;

namespace AspireMe.Infrastructure.Configuration;

public static class SerilogSetup
{
    public static void ConfigureSerilog(IHostApplicationBuilder builder)
    {
        builder.Services.AddSerilog((ctx, lc) => lc
                .Enrich.FromLogContext()
                .WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
                    var headers = builder.Configuration["OTEL_EXPORTER_OTLP_HEADERS"]?.Split(',') ?? [];
                    foreach (var header in headers)
                    {
                        var (key, value) = header.Split('=') switch
                        {
                            [string k, string v] => (k, v),
                            var v => throw new Exception($"Invalid header format {v}")
                        };

                        options.Headers.Add(key, value);
                    }
                    options.ResourceAttributes.Add("service.name", "apiservice");

                    //To remove the duplicate issue, we can use the below code to get the key and value from the configuration

                    var (otelResourceAttribute, otelResourceAttributeValue) = builder.Configuration["OTEL_RESOURCE_ATTRIBUTES"]?.Split('=') switch
                    {
                        [string k, string v] => (k, v),
                        _ => throw new Exception($"Invalid header format {builder.Configuration["OTEL_RESOURCE_ATTRIBUTES"]}")
                    };

                    options.ResourceAttributes.Add(otelResourceAttribute, otelResourceAttributeValue);
                })
                .ReadFrom.Configuration(builder.Configuration)
        );
    }
}
