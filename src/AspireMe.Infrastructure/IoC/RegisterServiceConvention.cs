using JasperFx.Core.TypeScanning;
using Lamar;
using Lamar.Scanning.Conventions;
using Microsoft.Extensions.DependencyInjection;

namespace AspireMe.Infrastructure.IoC;

internal class RegisterServiceConvention : IRegistrationConvention
{
    public void ScanTypes( TypeSet types, ServiceRegistry services )
    {
        // Find all concrete types decorated with [RegisterServiceAttribute]
        var candidates = types.FindTypes(
                TypeClassification.Concretes | TypeClassification.Closed )
            .Where( t => t.IsDefined( typeof( RegisterServiceAttribute ), false )
            );

        foreach ( var impl in candidates )
        {
            // Get the RegisterService attribute
            var attr = (RegisterServiceAttribute) impl
                .GetCustomAttributes( typeof( RegisterServiceAttribute ), false )
                .Single();

            // Register the type
            var interfaces = impl.GetInterfaces();
            var serviceType = interfaces.FirstOrDefault() ?? impl;

            var registration = services.For( serviceType ).Use( impl );

            switch ( attr.Lifetime )
            {
                case ServiceLifetime.Singleton:
                    registration.Singleton();
                    break;
                case ServiceLifetime.Scoped:
                    registration.Scoped();
                    break;
                case ServiceLifetime.Transient:
                default:
                    registration.Transient();
                    break;
            }
        }
    }
}

internal static class RegisterServiceConventionExtensions
{
    public static void WithRegisterServiceConventions( this IAssemblyScanner scanner )
    {
        scanner.With( new RegisterServiceConvention() );
    }
}

