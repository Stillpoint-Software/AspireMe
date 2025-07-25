
using System.Collections.Concurrent;
using Lamar;
using Lamar.Microsoft.DependencyInjection;
using AspireMe.Infrastructure.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace AspireMe.Infrastructure.Extensions;


public static class WebApplicationBuilderExtensions
{
    private static readonly ConcurrentDictionary<IHostApplicationBuilder, StartupRegistryCollection> _startupCollections = new();

    public static StartupRegistryCollection GetStartupRegistryCollection(this IHostApplicationBuilder builder )
    {
        return _startupCollections.GetOrAdd(builder, _ => new StartupRegistryCollection());
    }

    public static IHostApplicationBuilder UseStartup<T>(this IHostApplicationBuilder builder ) where T : IStartupRegistry
    {
        builder.GetStartupRegistryCollection().UseStartup<T>();
        return builder;
    }

    public static WebApplication ConfigureApplication(this WebApplicationBuilder builder, Action<IHostApplicationBuilder> configure)
    {
        // Allow user to register initial startups
        configure(builder);

        var collection = builder.GetStartupRegistryCollection();
        var processed = new HashSet<Type>();
        var toProcess = new Queue<IStartupRegistry>(collection.Startups);

        // Recursively process all startups, allowing them to add more
        while (toProcess.Count > 0)
        {
            var startup = toProcess.Dequeue();
            var type = startup.GetType();
            if (!processed.Add(type))
                continue;

            // Allow ConfigureServices to add more startups
            startup.ConfigureServices(builder, builder.Services);

            // Enqueue any new startups
            foreach (var s in collection.Startups)
            {
                if (!processed.Contains(s.GetType()) && !toProcess.Contains(s))
                    toProcess.Enqueue(s);
            }
        }

        builder.Host.UseLamar( registry =>
        {
            foreach ( var startup in collection.Startups.DistinctBy( s => s.GetType() ) )
            {
                startup.ConfigureScanner( registry );
            }
        } );

        var app = builder.Build();

        foreach (var startup in collection.Startups.DistinctBy(s => s.GetType()))
        {
            startup.ConfigureApp(app, app.Environment);
        }

        // Clean up the static dictionary
        _startupCollections.TryRemove(builder, out _);

        return app;
    }
}

public class StartupRegistryCollection
{
    private readonly List<IStartupRegistry> _startups = new();

    internal StartupRegistryCollection() { }

    public StartupRegistryCollection UseStartup<T>()
        where T : IStartupRegistry
    {
        var type = typeof(T);
        if (_startups.All(s => s.GetType() != type))
        {
            var instance = (IStartupRegistry)Activator.CreateInstance(type)!;
            _startups.Add(instance);
        }
        return this;
    }

    internal IReadOnlyList<IStartupRegistry> Startups => _startups;
}
