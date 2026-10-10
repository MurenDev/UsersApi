namespace UsersApi.Application.Users.Commands;

// IsActive es opcional: si no se envia se mantiene el valor que ya tenia el usuario
public record UpdateUserCommand(int Id, string Name, string Email, bool? IsActive);
