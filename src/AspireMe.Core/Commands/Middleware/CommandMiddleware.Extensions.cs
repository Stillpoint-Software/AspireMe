using System.Diagnostics;
using Hyperbee.Pipeline;
using AspireMe.Core.Extensions;
using AspireMe.Core.Services;
using Microsoft.Extensions.Logging;

namespace AspireMe.Core.Commands.Middleware;

[Flags]
public enum FormatOptions
{
    None = 0x0,
    InputOutput = Input | Output,
    Input = 0x1,
    Output = 0x2
}

public static class PipelineMiddleware
{
    public static IPipelineStartBuilder<TInput, TOutput> WithLogging<TInput, TOutput>( this IPipelineStartBuilder<TInput, TOutput> builder, LogLevel level = LogLevel.Information, FormatOptions options = FormatOptions.None )
    {
        return builder.HookAsync( async ( context, argument, next ) =>
        {
            context.Logger?.Log( level, "Command step [id={Id:D3}, name={Name}, action={Action}, options={Options}, input={@Input}]", 
                context.Id, 
                context.Name, 
                "Start", 
                options, 
                options.HasFlag( FormatOptions.Input ) ? argument : default 
            );

            var timer = Stopwatch.StartNew();

            var result = await next( context, argument ).ConfigureAwait( false );

            context.Logger?.Log( level, "Command step [id={Id:D3}, name={Name}, action={Action}, ms={Ms}, options={Options}, output={@Output}]", 
                context.Id, 
                context.Name, 
                "Stop", 
                timer.ElapsedMilliseconds, 
                options, options.HasFlag( FormatOptions.Output ) ? result : default 
            );

            return result;
        } );
    }

    public static IPipelineStartBuilder<TInput, TOutput> WithServiceExceptionHandling<TInput, TOutput>( this IPipelineStartBuilder<TInput, TOutput> builder, int errorcode = 0 ) // BF commandCode?
    {
        return builder.HookAsync( async ( context, argument, next ) =>
        {
            try
            {
                var result = await next( context, argument ).ConfigureAwait( false );
                return result;
            }
            catch ( ServiceException ex )
            {
                var message = string.Empty; // ErrorCodes.GetCommandErrorMessage( errorcode ); // lookup message string for provided errorcode
                context.Logger?.LogError( ex, message );
                context.FailAfter( $"{message} {ex.Message}", errorcode );
                return default;
            }
        } );
    }
}
