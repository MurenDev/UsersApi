using Microsoft.EntityFrameworkCore;
using UsersApi.Application.Common;
using UsersApi.Data;
using UsersApi.Models;

namespace UsersApi.Application.CurrencyConversion;

public class ConvertCurrencyHandler
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ConvertCurrencyHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<Result<ConvertCurrencyResponse>> Handle(ConvertCurrencyQuery query, CancellationToken ct = default)
    {
        var fromCode = query.FromCurrencyCode.Trim().ToUpperInvariant();
        var toCode = query.ToCurrencyCode.Trim().ToUpperInvariant();

        // se buscan las dos monedas en paralelo, cada consulta usa su propio DbContext
        var fromTask = GetCurrencyAsync(fromCode, ct);
        var toTask = GetCurrencyAsync(toCode, ct);
        await Task.WhenAll(fromTask, toTask);

        var from = fromTask.Result;
        var to = toTask.Result;

        if (from is null)
            return Result<ConvertCurrencyResponse>.NotFound($"No existe la moneda {fromCode}");
        if (to is null)
            return Result<ConvertCurrencyResponse>.NotFound($"No existe la moneda {toCode}");

        var montoBase = query.Amount * from.RateToBase;
        var convertedAmount = Math.Round(montoBase / to.RateToBase, 4);

        return Result<ConvertCurrencyResponse>.Ok(
            new ConvertCurrencyResponse(from.Code, to.Code, query.Amount, convertedAmount));
    }

    private async Task<Currency?> GetCurrencyAsync(string code, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.Currencies.AsNoTracking().FirstOrDefaultAsync(c => c.Code == code, ct);
    }
}
