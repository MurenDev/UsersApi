using Microsoft.EntityFrameworkCore;
using UsersApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

app.MapGet("/", () => "UsersApi funcionando");

app.Run();