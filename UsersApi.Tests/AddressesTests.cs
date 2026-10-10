using System.Net;
using System.Net.Http.Json;
using UsersApi.Application.Addresses;
using UsersApi.Application.Users;

namespace UsersApi.Tests;

public class AddressesTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public AddressesTests(ApiFactory factory)
    {
        _client = factory.CreateClientWithApiKey();
    }

    private async Task<UserResponse> CreateUserAsync(string email)
    {
        var response = await _client.PostAsJsonAsync("/users", new { name = "Usuario", email });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<UserResponse>())!;
    }

    private async Task<AddressResponse> CreateAddressAsync(int userId, string street = "Calle Falsa 123")
    {
        var response = await _client.PostAsJsonAsync($"/users/{userId}/addresses",
            new { street, city = "Asunción", country = "Paraguay", zipCode = "9999" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AddressResponse>())!;
    }

    [Fact]
    public async Task CreateAddress_ForExistingUser_ReturnsCreated()
    {
        var user = await CreateUserAsync("addr.crear@test.com");

        var response = await _client.PostAsJsonAsync($"/users/{user.Id}/addresses",
            new { street = "Calle Falsa 123", city = "Asunción", country = "Paraguay", zipCode = "9999" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var address = await response.Content.ReadFromJsonAsync<AddressResponse>();
        Assert.Equal(user.Id, address!.UserId);
        Assert.Equal("Calle Falsa 123", address.Street);
        Assert.Equal("Asunción", address.City);
    }

    [Fact]
    public async Task CreateAddress_WithoutZipCode_ReturnsCreated()
    {
        var user = await CreateUserAsync("addr.sinzip@test.com");

        var response = await _client.PostAsJsonAsync($"/users/{user.Id}/addresses",
            new { street = "Av. España 500", city = "Luque", country = "Paraguay" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var address = await response.Content.ReadFromJsonAsync<AddressResponse>();
        Assert.Null(address!.ZipCode);
    }

    [Fact]
    public async Task CreateAddress_ForNonExistingUser_ReturnsNotFound()
    {
        var response = await _client.PostAsJsonAsync("/users/999999/addresses",
            new { street = "Calle Falsa 123", city = "Asunción", country = "Paraguay" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateAddress_WithMissingFields_ReturnsBadRequest()
    {
        var user = await CreateUserAsync("addr.invalida@test.com");

        var response = await _client.PostAsJsonAsync($"/users/{user.Id}/addresses",
            new { street = "", city = "Asunción" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("La calle es obligatoria", body);
        Assert.Contains("El pais es obligatorio", body);
    }

    [Fact]
    public async Task GetAddresses_ReturnsOnlyAddressesOfThatUser()
    {
        var user = await CreateUserAsync("addr.listar@test.com");
        var otro = await CreateUserAsync("addr.otro@test.com");

        await CreateAddressAsync(user.Id, "Calle 1");
        await CreateAddressAsync(user.Id, "Calle 2");
        await CreateAddressAsync(otro.Id, "Calle de otro");

        var addresses = await _client.GetFromJsonAsync<List<AddressResponse>>($"/users/{user.Id}/addresses");

        Assert.Equal(2, addresses!.Count);
        Assert.All(addresses, a => Assert.Equal(user.Id, a.UserId));
    }

    [Fact]
    public async Task GetAddresses_ForNonExistingUser_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/users/999999/addresses");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAddress_WithValidData_UpdatesAddress()
    {
        var user = await CreateUserAsync("addr.update@test.com");
        var address = await CreateAddressAsync(user.Id);

        var response = await _client.PutAsJsonAsync($"/addresses/{address.Id}",
            new { street = "Otra calle 456", city = "Encarnación", country = "Paraguay", zipCode = "6000" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<AddressResponse>();
        Assert.Equal("Otra calle 456", updated!.Street);
        Assert.Equal("Encarnación", updated.City);
        Assert.Equal("6000", updated.ZipCode);
    }

    [Fact]
    public async Task UpdateAddress_WhenNotExists_ReturnsNotFound()
    {
        var response = await _client.PutAsJsonAsync("/addresses/999999",
            new { street = "Calle", city = "Ciudad", country = "Pais" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteAddress_WhenExists_ReturnsNoContent()
    {
        var user = await CreateUserAsync("addr.delete@test.com");
        var address = await CreateAddressAsync(user.Id);

        var response = await _client.DeleteAsync($"/addresses/{address.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var addresses = await _client.GetFromJsonAsync<List<AddressResponse>>($"/users/{user.Id}/addresses");
        Assert.Empty(addresses!);
    }

    [Fact]
    public async Task DeleteUser_AlsoDeletesItsAddresses()
    {
        var user = await CreateUserAsync("addr.cascada@test.com");
        var address = await CreateAddressAsync(user.Id);

        await _client.DeleteAsync($"/users/{user.Id}");

        // la direccion ya no existe porque se borro en cascada
        var response = await _client.DeleteAsync($"/addresses/{address.Id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
