using System.Diagnostics;
using AspireMe.Core.Commands;
using AspireMe.Core.Commands.Middleware;
using Azure.Messaging.ServiceBus;
using Hyperbee.Pipeline;
using Hyperbee.Pipeline.Commands;
using Hyperbee.Pipeline.Context;
using OpenTelemetry;

namespace AspireMe.Api.Commands.Messaging;

public interface IPublishCommand : ICommandFunction<string, string>;

public class PublishCommand : ServiceCommandFunction<string, string>, IPublishCommand
{
    private readonly string _user;
    private readonly ServiceBusClient _client;
    private readonly ITelemetrySourceProvider _telemetryProvider;

    public PublishCommand(
        ServiceBusClient client,
        IPipelineContextFactory pipelineContextFactory,
        ITelemetrySourceProvider telemetryProvider,
        ILogger<PublishCommand> logger ) :
        base( pipelineContextFactory, logger )
    {
        _client = client;
        _telemetryProvider = telemetryProvider;
    }

    protected override FunctionAsync<string, string> CreatePipeline()
    {
        return PipelineFactory
            .Start<string>()
            .WithLogging()
            .WithTelemetry( _telemetryProvider, activityName: "PublishMessage", activityKind: ActivityKind.Producer )
            .PipeAsync( CreatePublishAsync )
            .Build();
    }

    private async Task<string> CreatePublishAsync( IPipelineContext context, string message )
    {
        //var activity = Activity.Current;

        var sender = _client.CreateSender( "topic" );
        var newMessage = new ServiceBusMessage( message )
        {
            Body = new BinaryData( message ),
            CorrelationId = Guid.NewGuid().ToString(),
        };

        // Stash the message for telemetry middleware
        context.Items.SetValue( "ServiceBusMessage", newMessage );

        Logger.LogInformation( "Sending message with correlation Id {CorrelationId}", newMessage.CorrelationId );

        await sender.SendMessageAsync( newMessage );
        return "Published to topic!";
    }

}

