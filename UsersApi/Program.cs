using Microsoft.EntityFrameworkCore;
using UsersApi.Data;
using UsersApi.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

app.UseMiddleware<ApiKeyMiddleware>();

app.MapGet("/", () => "UsersApi funcionando");

app.Run();