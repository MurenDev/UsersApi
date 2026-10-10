using UsersApi.Application.Currencies.Commands;
using UsersApi.Application.CurrencyConversion;

namespace UsersApi.Tests.Unit;

public class CurrencyValidatorsTests
{
    private readonly CreateCurrencyValidator _createValidator = new();
    private readonly ConvertCurrencyValidator _convertValidator = new();

    [Theory]
    [InlineData("USD")]
    [InlineData("usd")]
    [InlineData("PYG")]
    public void CreateCurrency_WithValidCode_IsValid(string code)
    {
        var result = _createValidator.Validate(new CreateCurrencyCommand(code, "Moneda", 1m));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("US1")]
    [InlineData("U-S")]
    [InlineData("U S")]
    [InlineData("CODIGOLARGOO")]
    public void CreateCurrency_WithInvalidCode_IsInvalid(string code)
    {
        var result = _createValidator.Validate(new CreateCurrencyCommand(code, "Moneda", 1m));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateCurrency_WithEmptyCode_ReturnsOnlyOneError()
    {
        // si el codigo esta vacio alcanza con avisar que es obligatorio
        var result = _createValidator.Validate(new CreateCurrencyCommand("", "Moneda", 1m));

        var error = Assert.Single(result.Errors);
        Assert.Equal("El codigo es obligatorio", error.ErrorMessage);
    }

    [Fact]
    public void CreateCurrency_WithEmptyName_IsInvalid()
    {
        var result = _createValidator.Validate(new CreateCurrencyCommand("USD", "", 1m));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "El nombre es obligatorio");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateCurrency_WithRateNotGreaterThanZero_IsInvalid(decimal rate)
    {
        var result = _createValidator.Validate(new CreateCurrencyCommand("USD", "Dolar", rate));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "La tasa debe ser mayor a 0");
    }

    [Fact]
    public void CreateCurrency_WithDecimalRate_IsValid()
    {
        var result = _createValidator.Validate(new CreateCurrencyCommand("CLP", "Peso chileno", 7.8m));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Convert_WithValidData_IsValid()
    {
        var result = _convertValidator.Validate(new ConvertCurrencyQuery("USD", "PYG", 100m));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void Convert_WithAmountNotGreaterThanZero_IsInvalid(decimal amount)
    {
        var result = _convertValidator.Validate(new ConvertCurrencyQuery("USD", "PYG", amount));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "El monto debe ser mayor a 0");
    }

    [Fact]
    public void Convert_WithAmountOverTheLimit_IsInvalid()
    {
        var result = _convertValidator.Validate(
            new ConvertCurrencyQuery("USD", "PYG", ConvertCurrencyValidator.MaxAmount + 1));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Convert_WithAmountAtTheLimit_IsValid()
    {
        var result = _convertValidator.Validate(
            new ConvertCurrencyQuery("USD", "PYG", ConvertCurrencyValidator.MaxAmount));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Convert_WithoutCurrencyCodes_ReturnsBothErrors()
    {
        var result = _convertValidator.Validate(new ConvertCurrencyQuery("", "", 100m));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "La moneda de origen es obligatoria");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "La moneda de destino es obligatoria");
    }
}
