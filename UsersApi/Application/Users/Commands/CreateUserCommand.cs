namespace UsersApi.Application.Users.Commands;

public record CreateUserCommand(string Name, string Email, string? Password);