using Microsoft.EntityFrameworkCore;
using UsersApi.Models;

namespace UsersApi.Data;

public static class DbInitializer
{
    // aplica las migraciones pendientes y carga monedas de ejemplo si la tabla esta vacia
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var factory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var db = await factory.CreateDbContextAsync();

        await db.Database.MigrateAsync();

        if (!await db.Currencies.AnyAsync())
        {
            // moneda base: PYG = 1. Las tasas son de ejemplo
            db.Currencies.AddRange(
                new Currency { Code = "PYG", Name = "Guarani paraguayo", RateToBase = 1m },
                new Currency { Code = "USD", Name = "Dolar estadounidense", RateToBase = 7300m },
                new Currency { Code = "EUR", Name = "Euro", RateToBase = 8500m },
                new Currency { Code = "BRL", Name = "Real brasileno", RateToBase = 1350m },
                new Currency { Code = "ARS", Name = "Peso argentino", RateToBase = 5m });

            await db.SaveChangesAsync();
        }
    }
}
