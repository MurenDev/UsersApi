using FluentValidation;

namespace UsersApi.Application.Currencies.Commands;

public class CreateCurrencyValidator : AbstractValidator<CreateCurrencyCommand>
{
    public CreateCurrencyValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El codigo es obligatorio")
            .MaximumLength(10).WithMessage("El codigo no puede superar 10 caracteres");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres");

        RuleFor(x => x.RateToBase)
            .GreaterThan(0).WithMessage("La tasa debe ser mayor a 0");
    }
}
