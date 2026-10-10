using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using UsersApi.Application.Addresses.Commands;
using UsersApi.Application.Addresses.Queries;
using UsersApi.Application.Currencies.Commands;
using UsersApi.Application.Currencies.Queries;
using UsersApi.Application.CurrencyConversion;
using UsersApi.Application.Users.Commands;
using UsersApi.Application.Users.Queries;
using UsersApi.Data;
using UsersApi.Endpoints;
using UsersApi.Middleware;
using UsersApi.Security;

var builder = WebApplication.CreateBuilder(args);

// Se usa DbContextFactory en vez del DbContext scoped porque hay flujos en paralelo
// (carga masiva, conversion) y un DbContext no se puede compartir entre tareas
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();

// los errores de validacion salen con el nombre del campo igual que en el JSON (name, email)
ValidatorOptions.Global.PropertyNameResolver = (type, member, expression) =>
    member is null ? null : JsonNamingPolicy.CamelCase.ConvertName(member.Name);

// si el body no se puede leer se lanza la excepcion para que la maneje ErrorHandlingMiddleware
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

// Users
builder.Services.AddScoped<CreateUserHandler>();
builder.Services.AddScoped<UpdateUserHandler>();
builder.Services.AddScoped<DeleteUserHandler>();
builder.Services.AddScoped<BulkCreateUsersHandler>();
builder.Services.AddScoped<GetUsersHandler>();
builder.Services.AddScoped<GetUserByIdHandler>();

// Addresses
builder.Services.AddScoped<CreateAddressHandler>();
builder.Services.AddScoped<UpdateAddressHandler>();
builder.Services.AddScoped<DeleteAddressHandler>();
builder.Services.AddScoped<GetAddressesByUserHandler>();

// Currencies
builder.Services.AddScoped<CreateCurrencyHandler>();
builder.Services.AddScoped<GetCurrenciesHandler>();
builder.Services.AddScoped<ConvertCurrencyHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "UsersApi",
        Version = "v1",
        Description = "API de usuarios, direcciones y conversion de divisas"
    });

    // para poder cargar la API Key desde el boton Authorize de Swagger
    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-API-KEY",
        Description = "API Key configurada en appsettings.json"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("ApiKey", document)] = new List<string>()
    });
});

var app = builder.Build();

// crea la base si no existe y aplica las migraciones
await DbInitializer.InitializeAsync(app.Services);

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<ApiKeyMiddleware>();

app.MapGet("/", () => "UsersApi funcionando").ExcludeFromDescription();
app.MapUserEndpoints();
app.MapAddressEndpoints();
app.MapCurrencyEndpoints();

app.Run();
