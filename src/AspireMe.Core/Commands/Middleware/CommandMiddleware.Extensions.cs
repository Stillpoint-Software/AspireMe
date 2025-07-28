using System.Diagnostics;
using AspireMe.Core.Extensions;
using AspireMe.Core.Services;
using Azure.Messaging.ServiceBus;
using Hyperbee.Pipeline;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

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
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

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
    public static IPipelineStartBuilder<TInput, TOutput> WithTelemetry<TInput, TOutput>(
    this IPipelineStartBuilder<TInput, TOutput> builder,
    ITelemetrySourceProvider telemetryProvider,
    string activityName,
    ActivityKind activityKind = ActivityKind.Internal )
    {
        return builder.HookAsync( async ( context, argument, next ) =>
        {
            //Extracts incoming trace context on the consumer side
            PropagationContext parentContext = default;
            if ( activityKind == ActivityKind.Consumer )
            {
                parentContext = Propagator.Extract(
                    default,
                    context.Items,
                    ( items, key ) =>
                        items.TryGetValue<string>( key, out var raw ) ? new[] { raw } : Array.Empty<string>() );
                Baggage.Current = parentContext.Baggage;
            }

            // Starts the span
            using var activity = telemetryProvider
                .GetActivitySource()
                .StartActivity(
                    activityName,
                    activityKind,
                    activityKind == ActivityKind.Consumer ? parentContext.ActivityContext : default );

            if ( activity != null )
            {
                // Producer-side
                if ( activityKind == ActivityKind.Producer
                    && context.Items.TryGetValue<ServiceBusMessage>( "ServiceBusMessage", out var sbMsg ) )
                {
                    // propagate context
                    Propagator.Inject(
                        new PropagationContext( activity.Context, Baggage.Current ),
                        sbMsg.ApplicationProperties,
                        ( props, key, value ) => props[key] = value );

                    // Enrich span with message details
                    activity.SetTag( "messaging.correlation_id", sbMsg.CorrelationId );
                    if ( !string.IsNullOrEmpty( sbMsg.MessageId ) )
                        activity.SetTag( "messaging.message_id", sbMsg.MessageId );
                    var payload = sbMsg.Body.ToString();
                    activity.SetTag( "messaging.payload_size_bytes", sbMsg.Body.ToArray().Length );
                    activity.SetTag( "messaging.message_payload", payload );
                }

                activity.SetTag( "pipeline.name", context.Name );
                activity.SetTag( "messaging.system", "azure.servicebus" );
                activity.SetTag( "messaging.destination", "topic" );
                activity.SetTag( "messaging.destination_kind", "topic" );
                activity.SetTag(
                    "messaging.operation",
                    activityKind == ActivityKind.Producer ? "publish" : "receive" );

                // Consumer-side
                if ( activityKind == ActivityKind.Consumer
                    && context.Items.TryGetValue<ServiceBusReceivedMessage>( "ServiceBusReceivedMessage", out var recMsg ) )
                {
                    activity.SetTag( "messaging.message_id", recMsg.MessageId );
                    activity.SetTag( "messaging.payload_size_bytes", recMsg.Body.ToArray().Length );
                    var recPayload = recMsg.Body.ToString();
                    activity.SetTag( "messaging.message_payload", recPayload );
                }
            }

            try
            {
                var result = await next( context, argument ).ConfigureAwait( false );
                activity?.SetTag( "otel.status_code", "OK" );
                return result;
            }
            catch ( Exception ex )
            {
                if ( activity != null )
                {
                    activity.SetStatus( ActivityStatusCode.Error, ex.Message );
                    activity.SetTag( "otel.status_code", "ERROR" );
                    activity.SetTag( "otel.status_description", ex.Message );
                    activity.AddEvent( new ActivityEvent(
                        "exception",
                        DateTimeOffset.UtcNow,
                        new ActivityTagsCollection
                        {
                        { "exception.type", ex.GetType().FullName },
                        { "exception.message", ex.Message },
                        { "exception.stacktrace", ex.StackTrace ?? string.Empty }
                        } ) );
                }
                throw;
            }
        } );
    }

}
