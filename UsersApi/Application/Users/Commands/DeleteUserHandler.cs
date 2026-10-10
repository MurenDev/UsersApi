using Microsoft.EntityFrameworkCore;
using UsersApi.Application.Common;
using UsersApi.Data;

namespace UsersApi.Application.Users.Commands;

public class DeleteUserHandler
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public DeleteUserHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<Result<bool>> Handle(DeleteUserCommand command, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == command.Id, ct);
        if (user is null)
            return Result<bool>.NotFound("Usuario no encontrado");

        db.Users.Remove(user);
        await db.SaveChangesAsync(ct);

        return Result<bool>.Ok(true);
    }
}