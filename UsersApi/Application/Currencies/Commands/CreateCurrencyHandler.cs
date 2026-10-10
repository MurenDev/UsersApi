using Microsoft.EntityFrameworkCore;
using UsersApi.Application.Common;
using UsersApi.Data;
using UsersApi.Models;

namespace UsersApi.Application.Currencies.Commands;

public class CreateCurrencyHandler
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public CreateCurrencyHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<Result<CurrencyResponse>> Handle(CreateCurrencyCommand command, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        // los codigos se guardan siempre en mayusculas (USD, PYG)
        var code = command.Code.Trim().ToUpperInvariant();

        var exists = await db.Currencies.AnyAsync(c => c.Code == code, ct);
        if (exists)
            return Result<CurrencyResponse>.Conflict("Ya existe una moneda con ese codigo");

        var currency = new Currency
        {
            Code = code,
            Name = command.Name.Trim(),
            RateToBase = command.RateToBase
        };

        db.Currencies.Add(currency);
        await db.SaveChangesAsync(ct);

        return Result<CurrencyResponse>.Ok(CurrencyResponse.From(currency));
    }
}
