using UsersApi.Application.CurrencyConversion;

namespace UsersApi.Tests.Unit;

public class CurrencyCalculatorTests
{
    [Fact]
    public void Convert_UsdToPyg_MatchesExampleFromStatement()
    {
        // ejemplo del enunciado: 100 USD (tasa 7300) a PYG (tasa 1)
        var result = CurrencyCalculator.Convert(100m, 7300m, 1m);

        Assert.Equal(730000m, result);
    }

    [Fact]
    public void Convert_PygToUsd_DividesByTargetRate()
    {
        var result = CurrencyCalculator.Convert(730000m, 1m, 7300m);

        Assert.Equal(100m, result);
    }

    [Fact]
    public void Convert_BetweenTwoNonBaseCurrencies_GoesThroughBase()
    {
        // 10 USD = 73000 PYG, y 73000 PYG / 1350 = 54.0741 BRL
        var result = CurrencyCalculator.Convert(10m, 7300m, 1350m);

        Assert.Equal(54.0741m, result);
    }

    [Fact]
    public void Convert_SameCurrency_ReturnsSameAmount()
    {
        var result = CurrencyCalculator.Convert(250.5m, 7300m, 7300m);

        Assert.Equal(250.5m, result);
    }

    [Fact]
    public void Convert_RoundsToFourDecimals()
    {
        // 1000 / 7300 = 0.136986...
        var result = CurrencyCalculator.Convert(1000m, 1m, 7300m);

        Assert.Equal(0.1370m, result);
    }

    [Fact]
    public void Convert_WithDecimalRates_Works()
    {
        var result = CurrencyCalculator.Convert(100m, 7.8m, 1m);

        Assert.Equal(780m, result);
    }
}
