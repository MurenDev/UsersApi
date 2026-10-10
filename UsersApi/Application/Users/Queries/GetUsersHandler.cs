using Microsoft.EntityFrameworkCore;
using UsersApi.Data;

namespace UsersApi.Application.Users.Queries;

public class GetUsersHandler
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public GetUsersHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<List<UserResponse>> Handle(GetUsersQuery query, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var users = db.Users.AsNoTracking();

        if (query.IsActive.HasValue)
            users = users.Where(u => u.IsActive == query.IsActive.Value);

        return await users
            .OrderBy(u => u.Id)
            .Select(u => new UserResponse(u.Id, u.Name, u.Email, u.IsActive, u.CreatedAt))
            .ToListAsync(ct);
    }
}