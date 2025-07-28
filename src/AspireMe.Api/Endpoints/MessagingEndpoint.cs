using System.Diagnostics;
using AspireMe.Api.Commands.Messaging;
using AspireMe.Core.Commands.Middleware;
using AspireMe.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace AspireMe.Api.Endpoints;

public static class MessagingEndpoint
{
    public static RouteGroupBuilder MapMessagingEndpoints( this IEndpointRouteBuilder endpoints )
    {
        var group = endpoints.MapGroup( "/api/messaging" ).WithTags( "Messaging" );

        group.MapPost( "/publish", async (
            [FromServices] IPublishCommand command,
            [FromServices] ITelemetrySourceProvider telemetryProvider,
            [FromBody] string message, CancellationToken cancellationToken = default ) =>
        {
            using var activity = telemetryProvider
            .GetActivitySource()
            .StartActivity( "PublishMessage", ActivityKind.Producer );

            activity?.SetTag( "http.method", "POST-api/messaging/publish" );
            activity?.SetTag( "messaging.message_payload", message );

            var result = await command.ExecuteAsync( message, cancellationToken );

            return result.ToResult();

        } );

        group.MapGet( "/receive", async (
            [FromServices] IReceiverCommand receiver,
            [FromServices] ITelemetrySourceProvider telemetryProvider,
            CancellationToken cancellationToken = default ) =>
        {
            using var activity = telemetryProvider
             .GetActivitySource()
             .StartActivity( "ReceiveMessage", ActivityKind.Consumer );

            activity?.SetTag( "http.method", "GET-api/messaging/receive" );

            var result = await receiver.ExecuteAsync( cancellationToken );

            return result.ToResult();
        } );

        return group;
    }
}
