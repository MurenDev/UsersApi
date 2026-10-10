using Microsoft.EntityFrameworkCore;
using UsersApi.Application.Common;
using UsersApi.Data;

namespace UsersApi.Application.Addresses.Queries;

public class GetAddressesByUserHandler
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public GetAddressesByUserHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<Result<List<AddressResponse>>> Handle(GetAddressesByUserQuery query, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var userExists = await db.Users.AnyAsync(u => u.Id == query.UserId, ct);
        if (!userExists)
            return Result<List<AddressResponse>>.NotFound("Usuario no encontrado");

        var addresses = await db.Addresses
            .AsNoTracking()
            .Where(a => a.UserId == query.UserId)
            .OrderBy(a => a.Id)
            .Select(a => new AddressResponse(a.Id, a.UserId, a.Street, a.City, a.Country, a.ZipCode))
            .ToListAsync(ct);

        return Result<List<AddressResponse>>.Ok(addresses);
    }
}
