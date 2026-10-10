using Microsoft.EntityFrameworkCore;
using UsersApi.Application.Common;
using UsersApi.Data;

namespace UsersApi.Application.Users.Commands;

public class UpdateUserHandler
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public UpdateUserHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<Result<UserResponse>> Handle(UpdateUserCommand command, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == command.Id, ct);
        if (user is null)
            return Result<UserResponse>.NotFound("Usuario no encontrado");

        var email = command.Email.Trim().ToLowerInvariant();

        var emailTaken = await db.Users.AnyAsync(u => u.Email == email && u.Id != command.Id, ct);
        if (emailTaken)
            return Result<UserResponse>.Conflict("Ya existe otro usuario con ese email");

        user.Name = command.Name.Trim();
        user.Email = email;
        user.IsActive = command.IsActive;

        await db.SaveChangesAsync(ct);

        return Result<UserResponse>.Ok(UserResponse.From(user));
    }
}