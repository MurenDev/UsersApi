using FluentValidation;

namespace UsersApi.Application.Users.Commands;

public class BulkCreateUsersValidator : AbstractValidator<BulkCreateUsersCommand>
{
    public const int MaxUsers = 1000;

    public BulkCreateUsersValidator()
    {
        // aca solo se valida la lista. Cada usuario se valida por separado en el handler
        // para poder informar cuales fallaron sin rechazar toda la carga
        RuleFor(x => x.Users)
            .NotEmpty().WithMessage("Debe enviar al menos un usuario");

        RuleFor(x => x.Users)
            .Must(users => users.Count <= MaxUsers)
            .WithMessage($"No se pueden enviar mas de {MaxUsers} usuarios por carga")
            .When(x => x.Users is not null);
    }
}
