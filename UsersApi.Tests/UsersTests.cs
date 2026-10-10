using System.Net;
using System.Net.Http.Json;
using UsersApi.Application.Users;

namespace UsersApi.Tests;

public class UsersTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public UsersTests(ApiFactory factory)
    {
        _client = factory.CreateClientWithApiKey();
    }

    private async Task<UserResponse> CreateUserAsync(string name, string email)
    {
        var response = await _client.PostAsJsonAsync("/users", new { name, email });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<UserResponse>())!;
    }

    [Fact]
    public async Task CreateUser_WithValidData_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/users", new { name = "Juan", email = "Juan.Crear@Test.com" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(user);
        Assert.True(user.Id > 0);
        Assert.Equal("Juan", user.Name);
        Assert.Equal("juan.crear@test.com", user.Email); // se guarda en minusculas
        Assert.True(user.IsActive);
    }

    [Fact]
    public async Task CreateUser_WithInvalidData_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/users", new { name = "", email = "no-es-un-email" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("El nombre es obligatorio", body);
        Assert.Contains("El email no tiene un formato valido", body);
    }

    [Fact]
    public async Task CreateUser_WithExistingEmail_ReturnsConflict()
    {
        await CreateUserAsync("Ana", "repetido@test.com");

        var response = await _client.PostAsJsonAsync("/users", new { name = "Otra Ana", email = "REPETIDO@test.com" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithPassword_DoesNotReturnIt()
    {
        var response = await _client.PostAsJsonAsync("/users",
            new { name = "Pedro", email = "pedro.pass@test.com", password = "secreto123" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("password", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secreto123", body);
    }

    [Fact]
    public async Task GetUserById_WhenExists_ReturnsUser()
    {
        var created = await CreateUserAsync("Luis", "luis.get@test.com");

        var user = await _client.GetFromJsonAsync<UserResponse>($"/users/{created.Id}");

        Assert.NotNull(user);
        Assert.Equal(created.Id, user.Id);
        Assert.Equal("luis.get@test.com", user.Email);
    }

    [Fact]
    public async Task GetUserById_WhenNotExists_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/users/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_FilterByIsActive_ReturnsOnlyMatchingUsers()
    {
        var activo = await CreateUserAsync("Activo", "activo.filtro@test.com");
        var inactivo = await CreateUserAsync("Inactivo", "inactivo.filtro@test.com");

        await _client.PutAsJsonAsync($"/users/{inactivo.Id}",
            new { name = "Inactivo", email = "inactivo.filtro@test.com", isActive = false });

        var activos = await _client.GetFromJsonAsync<List<UserResponse>>("/users?isActive=true");
        var inactivos = await _client.GetFromJsonAsync<List<UserResponse>>("/users?isActive=false");

        Assert.Contains(activos!, u => u.Id == activo.Id);
        Assert.DoesNotContain(activos!, u => u.Id == inactivo.Id);
        Assert.Contains(inactivos!, u => u.Id == inactivo.Id);
        Assert.All(inactivos!, u => Assert.False(u.IsActive));
    }

    [Fact]
    public async Task UpdateUser_WithValidData_UpdatesUser()
    {
        var created = await CreateUserAsync("Marta", "marta.update@test.com");

        var response = await _client.PutAsJsonAsync($"/users/{created.Id}",
            new { name = "Marta Silva", email = "marta.silva.update@test.com", isActive = false });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.Equal("Marta Silva", user!.Name);
        Assert.Equal("marta.silva.update@test.com", user.Email);
        Assert.False(user.IsActive);
    }

    [Fact]
    public async Task UpdateUser_WithoutIsActive_KeepsCurrentValue()
    {
        var created = await CreateUserAsync("Carlos", "carlos.keep@test.com");

        await _client.PutAsJsonAsync($"/users/{created.Id}",
            new { name = "Carlos", email = "carlos.keep@test.com", isActive = false });

        // no se envia isActive, tiene que seguir en false
        var response = await _client.PutAsJsonAsync($"/users/{created.Id}",
            new { name = "Carlos Rios", email = "carlos.keep@test.com" });

        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.Equal("Carlos Rios", user!.Name);
        Assert.False(user.IsActive);
    }

    [Fact]
    public async Task UpdateUser_WithEmailOfAnotherUser_ReturnsConflict()
    {
        await CreateUserAsync("Uno", "uno.conflicto@test.com");
        var dos = await CreateUserAsync("Dos", "dos.conflicto@test.com");

        var response = await _client.PutAsJsonAsync($"/users/{dos.Id}",
            new { name = "Dos", email = "uno.conflicto@test.com" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_WhenNotExists_ReturnsNotFound()
    {
        var response = await _client.PutAsJsonAsync("/users/999999",
            new { name = "Nadie", email = "nadie@test.com" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_WhenExists_ReturnsNoContent()
    {
        var created = await CreateUserAsync("Borrar", "borrar@test.com");

        var response = await _client.DeleteAsync($"/users/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/users/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_WhenNotExists_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/users/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithInvalidJson_ReturnsBadRequest()
    {
        var content = new StringContent("{ \"name\": ", System.Text.Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/users", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
