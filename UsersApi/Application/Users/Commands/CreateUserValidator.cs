using FluentValidation;

namespace UsersApi.Application.Users.Commands;

public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop) // con el primer error alcanza, no hace falta sumar el de formato
            .NotEmpty().WithMessage("El email es obligatorio")
            .EmailAddress().WithMessage("El email no tiene un formato valido")
            .MaximumLength(200).WithMessage("El email no puede superar 200 caracteres");

        RuleFor(x => x.Password)
            .MinimumLength(6).WithMessage("El password debe tener al menos 6 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Password));
    }
}