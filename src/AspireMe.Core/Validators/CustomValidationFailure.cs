using FluentValidation.Results;

namespace AspireMe.Core.Validators;

public class CustomValidationFailure : ValidationFailure
{
    public CustomValidationFailure( string propertyName, string errorMessage, string errorCode ) : base( propertyName, errorMessage, errorCode )
    {
        ErrorCode = errorCode;
    }
}
