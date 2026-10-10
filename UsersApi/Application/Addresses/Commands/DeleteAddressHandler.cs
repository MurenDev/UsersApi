using Microsoft.EntityFrameworkCore;
using UsersApi.Application.Common;
using UsersApi.Data;

namespace UsersApi.Application.Addresses.Commands;

public class DeleteAddressHandler
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public DeleteAddressHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<Result<bool>> Handle(DeleteAddressCommand command, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var address = await db.Addresses.FirstOrDefaultAsync(a => a.Id == command.Id, ct);
        if (address is null)
            return Result<bool>.NotFound("Direccion no encontrada");

        db.Addresses.Remove(address);
        await db.SaveChangesAsync(ct);

        return Result<bool>.Ok(true);
    }
}
