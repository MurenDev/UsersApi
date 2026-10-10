using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace UsersApi.Tests;

// Levanta la API en memoria con una base SQLite temporal, asi los tests no tocan app.db
public class ApiFactory : WebApplicationFactory<Program>
{
    public const string ApiKey = "prueba123";

    private readonly string _dbPath =
        Path.Combine(Path.GetTempPath(), $"usersapi_tests_{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={_dbPath}");
    }

    public HttpClient CreateClientWithApiKey()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-API-KEY", ApiKey);
        return client;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        // hay que soltar las conexiones para poder borrar el archivo
        SqliteConnection.ClearAllPools();

        foreach (var file in new[] { _dbPath, _dbPath + "-shm", _dbPath + "-wal" })
        {
            if (File.Exists(file))
                File.Delete(file);
        }
    }
}
