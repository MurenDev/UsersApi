namespace UsersApi.Application.CurrencyConversion;

public record ConvertCurrencyQuery(string FromCurrencyCode, string ToCurrencyCode, decimal Amount);

public record ConvertCurrencyResponse(string FromCurrency, string ToCurrency, decimal OriginalAmount, decimal ConvertedAmount);
