using FluentValidation;

namespace UsersApi.Application.Addresses.Commands;

public class UpdateAddressValidator : AbstractValidator<UpdateAddressCommand>
{
    public UpdateAddressValidator()
    {
        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("La calle es obligatoria")
            .MaximumLength(200).WithMessage("La calle no puede superar 200 caracteres");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("La ciudad es obligatoria")
            .MaximumLength(100).WithMessage("La ciudad no puede superar 100 caracteres");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("El pais es obligatorio")
            .MaximumLength(100).WithMessage("El pais no puede superar 100 caracteres");

        RuleFor(x => x.ZipCode)
            .MaximumLength(20).WithMessage("El codigo postal no puede superar 20 caracteres");
    }
}
