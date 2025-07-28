using System.Diagnostics;
using AspireMe.Core.Commands;
using AspireMe.Core.Commands.Middleware;
using Azure.Messaging.ServiceBus;
using Hyperbee.Pipeline;
using Hyperbee.Pipeline.Commands;
using Hyperbee.Pipeline.Context;
using OpenTelemetry;
using static Hyperbee.Pipeline.Arg;

namespace AspireMe.Api.Commands.Messaging;

public interface IReceiverCommand : ICommandFunction<Empty, string>;

public class ReceiverCommand : ServiceCommandFunction<Empty, string>, IReceiverCommand
{
    private readonly ServiceBusClient _client;
    private readonly ITelemetrySourceProvider _telemetryProvider;

    public ReceiverCommand(
        ServiceBusClient client,
        IPipelineContextFactory pipelineContextFactory,
        ITelemetrySourceProvider telemetryProvider,
        ILogger<ReceiverCommand> logger
        )
        : base( pipelineContextFactory, logger )
    {
        _client = client;
        _telemetryProvider = telemetryProvider;
    }


    protected override FunctionAsync<Empty, string> CreatePipeline()
    {
        return PipelineFactory
            .Start<Empty>()
            .WithLogging()
            .WithTelemetry( _telemetryProvider, activityName: "ReceiveMessage", activityKind: ActivityKind.Consumer )
            .PipeAsync( ReceiverAsync )
            .Build();
    }

    private async Task<string> ReceiverAsync( IPipelineContext context, Empty _ )
    {
        var activity = Activity.Current;

        var receiver = _client.CreateReceiver(
                topicName: "topic",
                subscriptionName: "sub2" );

        var message = await receiver.ReceiveMessageAsync( TimeSpan.FromSeconds( 5 ) );
        if ( message == null )
            return "No messages in subscription.";

        // Stash the received message for telemetry middleware
        context.Items.SetValue( "ServiceBusReceivedMessage", message );

        var body = message.Body.ToString();
        Logger.LogInformation( "Received message with correlation Id {CorrelationId}", message.CorrelationId );

        await receiver.CompleteMessageAsync( message );
        return $"Received from subscription: {body}";
    }
}

