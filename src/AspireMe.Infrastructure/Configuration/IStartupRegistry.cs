using Lamar;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AspireMe.Infrastructure.Configuration;

public interface IStartupRegistry
{
    void ConfigureServices(IHostApplicationBuilder builder, IServiceCollection services);
    void ConfigureScanner(ServiceRegistry services);
    void ConfigureApp(WebApplication app, IWebHostEnvironment env);
}
