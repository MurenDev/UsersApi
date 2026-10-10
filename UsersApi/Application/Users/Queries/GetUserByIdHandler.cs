using Microsoft.EntityFrameworkCore;
using UsersApi.Application.Common;
using UsersApi.Data;

namespace UsersApi.Application.Users.Queries;

public class GetUserByIdHandler
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public GetUserByIdHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<Result<UserResponse>> Handle(GetUserByIdQuery query, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == query.Id, ct);

        if (user is null)
            return Result<UserResponse>.NotFound("Usuario no encontrado");

        return Result<UserResponse>.Ok(UserResponse.From(user));
    }
}