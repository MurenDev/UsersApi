using UsersApi.Application.Addresses.Commands;

namespace UsersApi.Tests.Unit;

public class AddressValidatorsTests
{
    private readonly CreateAddressValidator _createValidator = new();
    private readonly UpdateAddressValidator _updateValidator = new();

    [Fact]
    public void CreateAddress_WithValidData_IsValid()
    {
        var command = new CreateAddressCommand(1, "Calle Falsa 123", "Asunción", "Paraguay", "9999");

        var result = _createValidator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateAddress_WithoutZipCode_IsValid()
    {
        // el codigo postal es opcional
        var command = new CreateAddressCommand(1, "Calle Falsa 123", "Asunción", "Paraguay", null);

        var result = _createValidator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "Asunción", "Paraguay", "La calle es obligatoria")]
    [InlineData("Calle Falsa 123", "", "Paraguay", "La ciudad es obligatoria")]
    [InlineData("Calle Falsa 123", "Asunción", "", "El pais es obligatorio")]
    public void CreateAddress_WithMissingField_IsInvalid(string street, string city, string country, string expectedError)
    {
        var command = new CreateAddressCommand(1, street, city, country, null);

        var result = _createValidator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == expectedError);
    }

    [Fact]
    public void CreateAddress_WithZipCodeLongerThan20_IsInvalid()
    {
        var command = new CreateAddressCommand(1, "Calle Falsa 123", "Asunción", "Paraguay", new string('1', 21));

        var result = _createValidator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateAddress_WithValidData_IsValid()
    {
        var command = new UpdateAddressCommand(1, "Av. España 500", "Luque", "Paraguay", "2060");

        var result = _updateValidator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateAddress_WithAllFieldsEmpty_ReturnsThreeErrors()
    {
        var command = new UpdateAddressCommand(1, "", "", "", null);

        var result = _updateValidator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Equal(3, result.Errors.Count);
    }
}
