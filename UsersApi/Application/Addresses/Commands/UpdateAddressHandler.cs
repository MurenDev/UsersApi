using Microsoft.EntityFrameworkCore;
using UsersApi.Application.Common;
using UsersApi.Data;

namespace UsersApi.Application.Addresses.Commands;

public class UpdateAddressHandler
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public UpdateAddressHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<Result<AddressResponse>> Handle(UpdateAddressCommand command, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var address = await db.Addresses.FirstOrDefaultAsync(a => a.Id == command.Id, ct);
        if (address is null)
            return Result<AddressResponse>.NotFound("Direccion no encontrada");

        address.Street = command.Street.Trim();
        address.City = command.City.Trim();
        address.Country = command.Country.Trim();
        address.ZipCode = command.ZipCode?.Trim();

        await db.SaveChangesAsync(ct);

        return Result<AddressResponse>.Ok(AddressResponse.From(address));
    }
}
