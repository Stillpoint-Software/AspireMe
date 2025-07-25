using FluentValidation;

namespace AspireMe.Core.Validators;

public interface IValidatorProvider
{
    IValidator<TPlugin> For<TPlugin>()
        where TPlugin : class;
}
