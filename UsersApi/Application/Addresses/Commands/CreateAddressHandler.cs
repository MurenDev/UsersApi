using Microsoft.EntityFrameworkCore;
using UsersApi.Application.Common;
using UsersApi.Data;
using UsersApi.Models;

namespace UsersApi.Application.Addresses.Commands;

public class CreateAddressHandler
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public CreateAddressHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<Result<AddressResponse>> Handle(CreateAddressCommand command, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var userExists = await db.Users.AnyAsync(u => u.Id == command.UserId, ct);
        if (!userExists)
            return Result<AddressResponse>.NotFound("Usuario no encontrado");

        var address = new Address
        {
            UserId = command.UserId,
            Street = command.Street.Trim(),
            City = command.City.Trim(),
            Country = command.Country.Trim(),
            ZipCode = command.ZipCode?.Trim()
        };

        db.Addresses.Add(address);
        await db.SaveChangesAsync(ct);

        return Result<AddressResponse>.Ok(AddressResponse.From(address));
    }
}
