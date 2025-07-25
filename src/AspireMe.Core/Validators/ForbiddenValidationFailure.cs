using System.Net;
using FluentValidation.Results;

namespace AspireMe.Core.Validators;

public class ForbiddenValidationFailure : ValidationFailure
{
    public ForbiddenValidationFailure( string propertyName, string errorMessage ) : base( propertyName, errorMessage )
    {
        ErrorCode = nameof(HttpStatusCode.Forbidden);
    }
}
