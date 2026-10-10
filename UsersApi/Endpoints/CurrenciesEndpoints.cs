using FluentValidation;
using UsersApi.Application.Common;
using UsersApi.Application.Currencies;
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
        })
        .WithTags("Currencies")
        .WithSummary("Listar monedas")
        .Produces<List<CurrencyResponse>>();

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
                ErrorType.Conflict => Results.Conflict(new ErrorResponse(result.Message!)),
                _ => Results.Created("/currencies", result.Value)
            };
        })
        .WithTags("Currencies")
        .WithSummary("Crear moneda")
        .WithDescription("RateToBase es la tasa respecto a la moneda base (PYG = 1).")
        .Produces<CurrencyResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .Produces<ErrorResponse>(StatusCodes.Status409Conflict);

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
                ErrorType.NotFound => Results.NotFound(new ErrorResponse(result.Message!)),
                _ => Results.Ok(result.Value)
            };
        })
        .WithTags("Currencies")
        .WithSummary("Convertir un monto entre dos monedas")
        .WithDescription("Las dos monedas deben existir. Se calcula amount * from.RateToBase / to.RateToBase.")
        .Produces<ConvertCurrencyResponse>()
        .ProducesValidationProblem()
        .Produces<ErrorResponse>(StatusCodes.Status404NotFound);
    }
}
