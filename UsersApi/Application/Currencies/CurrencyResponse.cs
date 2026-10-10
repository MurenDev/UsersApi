using UsersApi.Models;

namespace UsersApi.Application.Currencies;

public record CurrencyResponse(int Id, string Code, string Name, decimal RateToBase)
{
    public static CurrencyResponse From(Currency currency) =>
        new(currency.Id, currency.Code, currency.Name, currency.RateToBase);
}
