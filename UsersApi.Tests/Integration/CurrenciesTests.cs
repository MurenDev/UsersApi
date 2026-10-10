using System.Net;
using System.Net.Http.Json;
using UsersApi.Application.Currencies;
using UsersApi.Application.CurrencyConversion;

namespace UsersApi.Tests;

public class CurrenciesTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public CurrenciesTests(ApiFactory factory)
    {
        _client = factory.CreateClientWithApiKey();
    }

    [Fact]
    public async Task GetCurrencies_ReturnsSeededCurrencies()
    {
        var currencies = await _client.GetFromJsonAsync<List<CurrencyResponse>>("/currencies");

        Assert.Contains(currencies!, c => c.Code == "PYG" && c.RateToBase == 1m);
        Assert.Contains(currencies!, c => c.Code == "USD");
    }

    [Fact]
    public async Task CreateCurrency_WithValidData_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/currencies",
            new { code = "clp", name = "Peso chileno", rateToBase = 7.8m });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var currency = await response.Content.ReadFromJsonAsync<CurrencyResponse>();
        Assert.Equal("CLP", currency!.Code); // se guarda en mayusculas
        Assert.Equal(7.8m, currency.RateToBase);
    }

    [Fact]
    public async Task CreateCurrency_WithExistingCode_ReturnsConflict()
    {
        var response = await _client.PostAsJsonAsync("/currencies",
            new { code = "usd", name = "Dolar", rateToBase = 7000m });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Theory]
    [InlineData("", "Nombre", 1)]      // sin codigo
    [InlineData("ABC", "", 1)]         // sin nombre
    [InlineData("ABC", "Nombre", 0)]   // tasa en cero
    [InlineData("ABC", "Nombre", -5)]  // tasa negativa
    [InlineData("AB1", "Nombre", 1)]   // codigo con numeros
    public async Task CreateCurrency_WithInvalidData_ReturnsBadRequest(string code, string name, decimal rateToBase)
    {
        var response = await _client.PostAsJsonAsync("/currencies", new { code, name, rateToBase });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Convert_UsdToPyg_ReturnsExpectedAmount()
    {
        // ejemplo del enunciado: 100 USD con tasa 7300 = 730000 PYG
        var response = await _client.PostAsJsonAsync("/currency/convert",
            new { fromCurrencyCode = "USD", toCurrencyCode = "PYG", amount = 100 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ConvertCurrencyResponse>();
        Assert.Equal("USD", result!.FromCurrency);
        Assert.Equal("PYG", result.ToCurrency);
        Assert.Equal(100m, result.OriginalAmount);
        Assert.Equal(730000m, result.ConvertedAmount);
    }

    [Fact]
    public async Task Convert_PygToUsd_ReturnsExpectedAmount()
    {
        var response = await _client.PostAsJsonAsync("/currency/convert",
            new { fromCurrencyCode = "PYG", toCurrencyCode = "USD", amount = 730000 });

        var result = await response.Content.ReadFromJsonAsync<ConvertCurrencyResponse>();
        Assert.Equal(100m, result!.ConvertedAmount);
    }

    [Fact]
    public async Task Convert_WithLowercaseCodes_Works()
    {
        var response = await _client.PostAsJsonAsync("/currency/convert",
            new { fromCurrencyCode = "usd", toCurrencyCode = "pyg", amount = 1 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ConvertCurrencyResponse>();
        Assert.Equal(7300m, result!.ConvertedAmount);
    }

    [Fact]
    public async Task Convert_WithNonExistingCurrency_ReturnsNotFound()
    {
        var response = await _client.PostAsJsonAsync("/currency/convert",
            new { fromCurrencyCode = "USD", toCurrencyCode = "XXX", amount = 100 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("XXX", body);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task Convert_WithAmountNotGreaterThanZero_ReturnsBadRequest(decimal amount)
    {
        var response = await _client.PostAsJsonAsync("/currency/convert",
            new { fromCurrencyCode = "USD", toCurrencyCode = "PYG", amount });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
