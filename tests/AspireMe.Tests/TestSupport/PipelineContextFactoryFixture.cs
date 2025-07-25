using AspireMe.Core.Identity;
using AspireMe.Core.Validators;
using Hyperbee.Pipeline.Context;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace AspireMe.Tests.TestSupport;

public class PipelineContextFactoryFixture
{
    public static IPipelineContextFactory Next(
        ServiceCollection services = default,
        IPrincipalProvider principalProvider = default,
        IValidatorProvider validatorProvider = default
    )
    {
        services ??= new ServiceCollection();
        services.AddTransient( _ => principalProvider ?? PrincipalFixture.Next() );
        services.AddTransient( _ => validatorProvider ?? Substitute.For<IValidatorProvider>() );
        return PipelineContextFactory.CreateFactory( services.BuildServiceProvider() );
    }
}
