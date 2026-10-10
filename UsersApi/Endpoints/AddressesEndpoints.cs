using FluentValidation;
using UsersApi.Application.Addresses;
using UsersApi.Application.Addresses.Commands;
using UsersApi.Application.Addresses.Queries;
using UsersApi.Application.Common;

namespace UsersApi.Endpoints;

public record AddressRequest(string Street, string City, string Country, string? ZipCode);

public static class AddressEndpoints
{
    public static void MapAddressEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/users/{userId:int}/addresses", async (
            int userId,
            AddressRequest request,
            IValidator<CreateAddressCommand> validator,
            CreateAddressHandler handler,
            CancellationToken ct) =>
        {
            var command = new CreateAddressCommand(userId, request.Street, request.City, request.Country, request.ZipCode);

            var validation = await validator.ValidateAsync(command, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var result = await handler.Handle(command, ct);

            return result.Error switch
            {
                ErrorType.NotFound => Results.NotFound(new ErrorResponse(result.Message!)),
                _ => Results.Created($"/users/{userId}/addresses", result.Value)
            };
        })
        .WithTags("Addresses")
        .WithSummary("Crear direccion para un usuario")
        .Produces<AddressResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        app.MapGet("/users/{userId:int}/addresses", async (int userId, GetAddressesByUserHandler handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new GetAddressesByUserQuery(userId), ct);

            return result.Error switch
            {
                ErrorType.NotFound => Results.NotFound(new ErrorResponse(result.Message!)),
                _ => Results.Ok(result.Value)
            };
        })
        .WithTags("Addresses")
        .WithSummary("Listar direcciones de un usuario")
        .Produces<List<AddressResponse>>()
        .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        app.MapPut("/addresses/{id:int}", async (
            int id,
            AddressRequest request,
            IValidator<UpdateAddressCommand> validator,
            UpdateAddressHandler handler,
            CancellationToken ct) =>
        {
            var command = new UpdateAddressCommand(id, request.Street, request.City, request.Country, request.ZipCode);

            var validation = await validator.ValidateAsync(command, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var result = await handler.Handle(command, ct);

            return result.Error switch
            {
                ErrorType.NotFound => Results.NotFound(new ErrorResponse(result.Message!)),
                _ => Results.Ok(result.Value)
            };
        })
        .WithTags("Addresses")
        .WithSummary("Modificar direccion")
        .Produces<AddressResponse>()
        .ProducesValidationProblem()
        .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        app.MapDelete("/addresses/{id:int}", async (int id, DeleteAddressHandler handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new DeleteAddressCommand(id), ct);

            return result.Error switch
            {
                ErrorType.NotFound => Results.NotFound(new ErrorResponse(result.Message!)),
                _ => Results.NoContent()
            };
        })
        .WithTags("Addresses")
        .WithSummary("Eliminar direccion")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ErrorResponse>(StatusCodes.Status404NotFound);
    }
}
