using System.Reflection;
using Hyperbee.Pipeline.Commands;
using Hyperbee.Pipeline.Context;
using AspireMe.Core.Extensions;
using AspireMe.Core.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AspireMe.Infrastructure.Extensions;

public static class CommandResultExtensions
{
    public static IResult ToResult<T>( this CommandResult<T> commandResult )
    {
        if ( TryHandleInvalidCommand( commandResult.Context, commandResult.CommandType, out var errorResult ) )
            return errorResult;

        return Results.Ok( commandResult.Result );
    }

    public static IResult ToResult( this CommandResult commandResult )
    {
        if ( TryHandleInvalidCommand( commandResult.Context, commandResult.CommandType, out var errorResult ) )
            return errorResult;

        return Results.Ok();
    }

    public static IResult ToFileResult( this CommandResult<Stream> commandResult, string contentType = "application/json" )
    {
        if ( TryHandleInvalidCommand( commandResult.Context, commandResult.CommandType, out var errorResult ) )
            return errorResult;

        return Results.File( commandResult.Result, contentType );
    }

    private static bool TryHandleInvalidCommand( IPipelineContext context, MemberInfo commandType, out IResult errorResult )
    {
        if ( !context.IsValid() )
        {
            if ( context.ValidationFailures().OfType<ForbiddenValidationFailure>().Any() )
            {
                errorResult = Results.Forbid();
                return true;
            }

            if ( context.ValidationFailures().OfType<UnauthorizedValidationFailure>().Any() )
            {
                errorResult = Results.Unauthorized();
                return true;
            }

            if ( context.ValidationFailures().OfType<CustomValidationFailure>().Any() )
            {
                errorResult = Results.BadRequest( context.ValidationFailures().OfType<CustomValidationFailure>().Select( x => new
                {
                    x.ErrorCode,
                    x.ErrorMessage
                } ) );
                return true;
            }

            errorResult = Results.BadRequest( context.ValidationFailures().Select( x => new
            {
                x.PropertyName,
                x.ErrorMessage,
                x.ErrorCode
            } ) );

            context.Logger?.LogInformation( "Command [name={Name}, action={Action}, validationResult={@ValidationResult}]",
                commandType.Name, "Validation", errorResult 
            );

            return true;
        }

        if ( context.IsCanceled && context.CancellationValue is IResult cancellationValue )
        {
            errorResult = cancellationValue;

            context.Logger?.LogInformation( "Command [name={Name}, action={Action}, cancellationResult={@CancellationResult}]",
                commandType.Name, "Canceled", errorResult 
            );
            return true;
        }

        if ( context.IsError )
            throw new CommandException( $"Command {commandType.Name} failed", context.Exception );

        errorResult = null;
        return false;
    }
}
