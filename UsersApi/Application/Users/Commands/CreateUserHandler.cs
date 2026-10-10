using Microsoft.EntityFrameworkCore;
using UsersApi.Application.Common;
using UsersApi.Data;
using UsersApi.Models;

namespace UsersApi.Application.Users.Commands;

public class CreateUserHandler
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public CreateUserHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<Result<UserResponse>> Handle(CreateUserCommand command, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var email = command.Email.Trim().ToLowerInvariant();

        var exists = await db.Users.AnyAsync(u => u.Email == email, ct);
        if (exists)
            return Result<UserResponse>.Conflict("Ya existe un usuario con ese email");

        var user = new User
        {
            Name = command.Name.Trim(),
            Email = email,
            PasswordHash = string.IsNullOrWhiteSpace(command.Password)
                ? null
                : BCrypt.Net.BCrypt.HashPassword(command.Password)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        return Result<UserResponse>.Ok(UserResponse.From(user));
    }
}