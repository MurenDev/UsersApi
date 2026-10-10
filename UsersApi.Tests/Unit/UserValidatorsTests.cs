using UsersApi.Application.Users.Commands;

namespace UsersApi.Tests.Unit;

public class UserValidatorsTests
{
    private readonly CreateUserValidator _createValidator = new();
    private readonly UpdateUserValidator _updateValidator = new();
    private readonly BulkCreateUsersValidator _bulkValidator = new();

    [Fact]
    public void CreateUser_WithValidData_IsValid()
    {
        var result = _createValidator.Validate(new CreateUserCommand("Juan", "juan@test.com", null));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateUser_WithEmptyName_IsInvalid(string? name)
    {
        var result = _createValidator.Validate(new CreateUserCommand(name!, "juan@test.com", null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "El nombre es obligatorio");
    }

    [Theory]
    [InlineData("")]
    [InlineData("sin-arroba")]
    [InlineData("@test.com")]
    [InlineData("juan@")]
    public void CreateUser_WithInvalidEmail_IsInvalid(string email)
    {
        var result = _createValidator.Validate(new CreateUserCommand("Juan", email, null));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateUser_WithNameLongerThan100_IsInvalid()
    {
        var name = new string('a', 101);

        var result = _createValidator.Validate(new CreateUserCommand(name, "juan@test.com", null));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateUser_WithShortPassword_IsInvalid()
    {
        var result = _createValidator.Validate(new CreateUserCommand("Juan", "juan@test.com", "123"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "El password debe tener al menos 6 caracteres");
    }

    [Fact]
    public void CreateUser_WithoutPassword_IsValid()
    {
        // el password es opcional
        var result = _createValidator.Validate(new CreateUserCommand("Juan", "juan@test.com", ""));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateUser_WithValidData_IsValid()
    {
        var result = _updateValidator.Validate(new UpdateUserCommand(1, "Juan", "juan@test.com", null));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateUser_WithEmptyNameAndInvalidEmail_ReturnsBothErrors()
    {
        var result = _updateValidator.Validate(new UpdateUserCommand(1, "", "no-es-email", true));

        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void Bulk_WithEmptyList_IsInvalid()
    {
        var result = _bulkValidator.Validate(new BulkCreateUsersCommand(new List<CreateUserCommand>()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Debe enviar al menos un usuario");
    }

    [Fact]
    public void Bulk_WithNullList_IsInvalid()
    {
        var result = _bulkValidator.Validate(new BulkCreateUsersCommand(null!));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Bulk_WithMoreUsersThanAllowed_IsInvalid()
    {
        var users = Enumerable.Range(1, BulkCreateUsersValidator.MaxUsers + 1)
            .Select(i => new CreateUserCommand($"Usuario {i}", $"usuario{i}@test.com", null))
            .ToList();

        var result = _bulkValidator.Validate(new BulkCreateUsersCommand(users));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Bulk_WithInvalidUserInside_IsStillValid()
    {
        // el validador de la carga solo revisa la lista. Cada usuario se valida despues en el handler
        var users = new List<CreateUserCommand> { new("", "email-malo", null) };

        var result = _bulkValidator.Validate(new BulkCreateUsersCommand(users));

        Assert.True(result.IsValid);
    }
}
