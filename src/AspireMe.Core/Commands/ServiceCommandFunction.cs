using FluentValidation;
using Hyperbee.Pipeline.Commands;
using Hyperbee.Pipeline.Context;
using AspireMe.Core.Validators;
using Microsoft.Extensions.Logging;

namespace AspireMe.Core.Commands;

public abstract class ServiceCommandFunction<TInput, TOutput>(
    IPipelineContextFactory pipelineContextFactory,
    ILogger logger )
    : CommandFunction<TInput, TOutput>( pipelineContextFactory, logger )
{
    public static IValidator Validate<T>( IValidatorProvider provider ) where T : class => provider.For<T>();
}

public abstract class ServiceCommandFunction<TOutput>(
    IPipelineContextFactory pipelineContextFactory,
    ILogger logger )
    : CommandFunction<TOutput>( pipelineContextFactory, logger );
