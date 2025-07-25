using FluentValidation.Results;
using Hyperbee.Pipeline.Context;
using AspireMe.Core.Extensions;
using AspireMe.Core.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AspireMe.Core.Commands.Middleware;

public static class PipelineContextUserExtensions
{
    public static string GetUserEmail( this IPipelineContext context )
    {
        var principalProvider = context.ServiceProvider.GetService<IPrincipalProvider>();

        var email = principalProvider?.GetEmail();
        
        if ( email != null )
            return email;

        context.AddValidationResult( new ValidationFailure( "User", "Invalid User" ) );
        context.CancelAfter();

        return null;
    }
}
