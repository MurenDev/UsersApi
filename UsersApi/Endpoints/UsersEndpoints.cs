using FluentValidation;
using UsersApi.Application.Common;
using UsersApi.Application.Users;
using UsersApi.Application.Users.Commands;
using UsersApi.Application.Users.Queries;

namespace UsersApi.Endpoints;

public record UpdateUserRequest(string Name, string Email, bool? IsActive);

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users").WithTags("Users");

        group.MapPost("/", async (
            CreateUserCommand command,
            IValidator<CreateUserCommand> validator,
            CreateUserHandler handler,
            CancellationToken ct) =>
        {
            var validation = await validator.ValidateAsync(command, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var result = await handler.Handle(command, ct);

            return result.Error switch
            {
                ErrorType.Conflict => Results.Conflict(new ErrorResponse(result.Message!)),
                _ => Results.Created($"/users/{result.Value!.Id}", result.Value)
            };
        })
        .WithSummary("Crear usuario")
        .WithDescription("El password es opcional. Si se envia se guarda hasheado.")
        .Produces<UserResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .Produces<ErrorResponse>(StatusCodes.Status409Conflict);

        group.MapGet("/", async (bool? isActive, GetUsersHandler handler, CancellationToken ct) =>
        {
            var users = await handler.Handle(new GetUsersQuery(isActive), ct);
            return Results.Ok(users);
        })
        .WithSummary("Listar usuarios")
        .WithDescription("Se puede filtrar con isActive=true o isActive=false.")
        .Produces<List<UserResponse>>();

        group.MapGet("/{id:int}", async (int id, GetUserByIdHandler handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new GetUserByIdQuery(id), ct);

            return result.Error switch
            {
                ErrorType.NotFound => Results.NotFound(new ErrorResponse(result.Message!)),
                _ => Results.Ok(result.Value)
            };
        })
        .WithSummary("Obtener usuario por id")
        .Produces<UserResponse>()
        .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPut("/{id:int}", async (
            int id,
            UpdateUserRequest request,
            IValidator<UpdateUserCommand> validator,
            UpdateUserHandler handler,
            CancellationToken ct) =>
        {
            var command = new UpdateUserCommand(id, request.Name, request.Email, request.IsActive);

            var validation = await validator.ValidateAsync(command, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var result = await handler.Handle(command, ct);

            return result.Error switch
            {
                ErrorType.NotFound => Results.NotFound(new ErrorResponse(result.Message!)),
                ErrorType.Conflict => Results.Conflict(new ErrorResponse(result.Message!)),
                _ => Results.Ok(result.Value)
            };
        })
        .WithSummary("Modificar usuario")
        .WithDescription("Si no se envia isActive se mantiene el valor actual.")
        .Produces<UserResponse>()
        .ProducesValidationProblem()
        .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:int}", async (int id, DeleteUserHandler handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new DeleteUserCommand(id), ct);

            return result.Error switch
            {
                ErrorType.NotFound => Results.NotFound(new ErrorResponse(result.Message!)),
                _ => Results.NoContent()
            };
        })
        .WithSummary("Eliminar usuario")
        .WithDescription("Tambien se eliminan las direcciones del usuario.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/bulk", async (
            BulkCreateUsersCommand command,
            IValidator<BulkCreateUsersCommand> validator,
            BulkCreateUsersHandler handler,
            CancellationToken ct) =>
        {
            var validation = await validator.ValidateAsync(command, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var result = await handler.Handle(command, ct);
            return Results.Ok(result);
        })
        .WithSummary("Carga masiva de usuarios")
        .WithDescription("Los usuarios se procesan en paralelo. La respuesta indica cuantos se crearon y cuales fallaron con el motivo.")
        .Produces<BulkCreateUsersResponse>()
        .ProducesValidationProblem();
    }
}
