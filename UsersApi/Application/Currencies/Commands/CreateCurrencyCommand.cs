namespace UsersApi.Application.Currencies.Commands;

public record CreateCurrencyCommand(string Code, string Name, decimal RateToBase);
