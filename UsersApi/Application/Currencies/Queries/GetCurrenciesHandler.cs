using Microsoft.EntityFrameworkCore;
using UsersApi.Data;

namespace UsersApi.Application.Currencies.Queries;

public class GetCurrenciesHandler
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public GetCurrenciesHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<List<CurrencyResponse>> Handle(GetCurrenciesQuery query, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        return await db.Currencies
            .AsNoTracking()
            .OrderBy(c => c.Code)
            .Select(c => new CurrencyResponse(c.Id, c.Code, c.Name, c.RateToBase))
            .ToListAsync(ct);
    }
}
