using AspireMe.Core.Extensions;
using Hyperbee.Pipeline.Commands;

namespace  AspireMe.Tests.TestSupport;

public static class CommandResultExtensions
{
    public static string ContextMessage( this CommandResult result )
    {
        if ( result?.Context == null )
        {
            return "Empty Result/Context";
        }

        if ( result.Context.Success )
        {
            return "Success";
        }

        if ( result.Context.Exception != null )
            return result.Context.Exception.Message;

        var validations = result.Context.GetValidationResult();
        if ( !validations.IsValid )
        {
            return string.Join( Environment.NewLine,
                validations.Errors.Select( x => $"{x.PropertyName}: {x.ErrorMessage} [{x.ErrorCode}]" ) );
        }

        return null;
    }
}
