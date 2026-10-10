using FluentValidation;
using Microsoft.EntityFrameworkCore;
using UsersApi.Application.Users.Commands;
using UsersApi.Application.Users.Queries;
using UsersApi.Data;
using UsersApi.Endpoints;
using UsersApi.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();

builder.Services.AddScoped<CreateUserHandler>();
builder.Services.AddScoped<UpdateUserHandler>();
builder.Services.AddScoped<DeleteUserHandler>();
builder.Services.AddScoped<GetUsersHandler>();
builder.Services.AddScoped<GetUserByIdHandler>();

var app = builder.Build();

app.UseMiddleware<ApiKeyMiddleware>();

app.MapGet("/", () => "UsersApi funcionando");
app.MapUserEndpoints();

app.Run();