using System.Net;
using System.Net.Http.Json;
using System.Text;
using UsersApi.Application.Users;
using UsersApi.Application.Users.Commands;

namespace UsersApi.Tests;

public class BulkUsersTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public BulkUsersTests(ApiFactory factory)
    {
        _client = factory.CreateClientWithApiKey();
    }

    [Fact]
    public async Task Bulk_WithSampleFile_CreatesAllUsers()
    {
        // es el mismo archivo que esta en sample-data/bulk_users_test.json
        var json = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "sample-data", "bulk_users_test.json"));
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/users/bulk", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<BulkCreateUsersResponse>();
        Assert.Equal(4, result!.Total);
        Assert.Equal(4, result.Created);
        Assert.Equal(0, result.Failed);
        Assert.Empty(result.Errors);
        Assert.Contains(result.CreatedUsers, u => u.Email == "ana.gomez@test.com");
    }

    [Fact]
    public async Task Bulk_WithInvalidAndDuplicatedUsers_ReportsWhichFailed()
    {
        var body = new
        {
            users = new object[]
            {
                new { name = "Valido", email = "valido.bulk@test.com" },
                new { name = "", email = "email-malo" },
                new { name = "Repetido", email = "repetido.bulk@test.com" },
                new { name = "Repetido otra vez", email = "REPETIDO.bulk@test.com" }
            }
        };

        var response = await _client.PostAsJsonAsync("/users/bulk", body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<BulkCreateUsersResponse>();
        Assert.Equal(4, result!.Total);
        Assert.Equal(2, result.Created);
        Assert.Equal(2, result.Failed);

        // el usuario invalido esta en la posicion 1 del arreglo
        var invalido = Assert.Single(result.Errors, e => e.Index == 1);
        Assert.Contains("El nombre es obligatorio", invalido.Error);

        // de los dos repetidos solo se crea uno, el otro falla por email duplicado
        var duplicado = Assert.Single(result.Errors, e => e.Index is 2 or 3);
        Assert.Equal("Ya existe un usuario con ese email", duplicado.Error);
    }

    [Fact]
    public async Task Bulk_WithEmptyList_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/users/bulk", new { users = Array.Empty<object>() });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Bulk_WithManyUsers_CreatesAllInParallel()
    {
        // 200 usuarios para comprobar que el procesamiento en paralelo no pierde ni duplica registros
        var users = Enumerable.Range(1, 200)
            .Select(i => new { name = $"Usuario {i}", email = $"paralelo{i}@test.com" })
            .ToList();

        var response = await _client.PostAsJsonAsync("/users/bulk", new { users });

        var result = await response.Content.ReadFromJsonAsync<BulkCreateUsersResponse>();
        Assert.Equal(200, result!.Created);
        Assert.Equal(0, result.Failed);

        var all = await _client.GetFromJsonAsync<List<UserResponse>>("/users");
        Assert.Equal(200, all!.Count(u => u.Email.StartsWith("paralelo")));
    }
}
