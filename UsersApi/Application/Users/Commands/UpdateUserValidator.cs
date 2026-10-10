using FluentValidation;

namespace UsersApi.Application.Users.Commands;

public class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio")
            .EmailAddress().WithMessage("El email no tiene un formato valido")
            .MaximumLength(200).WithMessage("El email no puede superar 200 caracteres");
    }
}