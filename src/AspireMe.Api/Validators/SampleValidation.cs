using FluentValidation;
using AspireMe.Data.Abstractions.Entity;

namespace AspireMe.Api.Validators;

public class SampleValidation : AbstractValidator<Sample>
{
    public SampleValidation()
    {
        RuleFor( x => x.Name )
            .NotEmpty()
            .NotNull()
            .WithMessage( $"{nameof( Sample.Name )} cannot be null or empty." );

    }
}
