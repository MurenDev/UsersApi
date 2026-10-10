using FluentValidation;
using UsersApi.Application.Common;
using UsersApi.Application.Currencies.Commands;
using UsersApi.Application.Currencies.Queries;
using UsersApi.Application.CurrencyConversion;

namespace UsersApi.Endpoints;

public static class CurrencyEndpoints
{
    public static void MapCurrencyEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/currencies", async (GetCurrenciesHandler handler, CancellationToken ct) =>
        {
            var currencies = await handler.Handle(new GetCurrenciesQuery(), ct);
            return Results.Ok(currencies);
        }).WithTags("Currencies");

        app.MapPost("/currencies", async (
            CreateCurrencyCommand command,
            IValidator<CreateCurrencyCommand> validator,
            CreateCurrencyHandler handler,
            CancellationToken ct) =>
        {
            var validation = await validator.ValidateAsync(command, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var result = await handler.Handle(command, ct);

            return result.Error switch
            {
                ErrorType.Conflict => Results.Conflict(new { error = result.Message }),
                _ => Results.Created("/currencies", result.Value)
            };
        }).WithTags("Currencies");

        app.MapPost("/currency/convert", async (
            ConvertCurrencyQuery query,
            IValidator<ConvertCurrencyQuery> validator,
            ConvertCurrencyHandler handler,
            CancellationToken ct) =>
        {
            var validation = await validator.ValidateAsync(query, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var result = await handler.Handle(query, ct);

            return result.Error switch
            {
                ErrorType.NotFound => Results.NotFound(new { error = result.Message }),
                _ => Results.Ok(result.Value)
            };
        }).WithTags("Currencies");
    }
}
