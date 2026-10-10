using FluentValidation;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using UsersApi.Data;
using UsersApi.Models;

namespace UsersApi.Application.Users.Commands;

public class BulkCreateUsersHandler
{
    // cantidad maxima de usuarios que se procesan al mismo tiempo
    private const int MaxParallel = 10;

    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly IValidator<CreateUserCommand> _validator;

    public BulkCreateUsersHandler(IDbContextFactory<AppDbContext> factory, IValidator<CreateUserCommand> validator)
    {
        _factory = factory;
        _validator = validator;
    }

    public async Task<BulkCreateUsersResponse> Handle(BulkCreateUsersCommand command, CancellationToken ct = default)
    {
        // se procesan los usuarios en paralelo, cada uno con su propio DbContext.
        // El semaforo limita cuantos corren a la vez para no abrir cientos de conexiones juntas
        using var semaphore = new SemaphoreSlim(MaxParallel);

        // Task.Run es necesario porque el proveedor de SQLite ejecuta las llamadas async
        // de forma sincrona. Sin esto cada usuario se procesaria completo antes de empezar el siguiente
        var tasks = command.Users.Select((user, index) => Task.Run(async () =>
        {
            await semaphore.WaitAsync(ct);
            try
            {
                return await CreateOneAsync(user, index, ct);
            }
            finally
            {
                semaphore.Release();
            }
        }, ct));

        var results = await Task.WhenAll(tasks);

        var created = results.Where(r => r.User is not null).Select(r => r.User!).ToList();
        var errors = results.Where(r => r.Error is not null).Select(r => r.Error!).ToList();

        return new BulkCreateUsersResponse(command.Users.Count, created.Count, errors.Count, created, errors);
    }

    private async Task<(UserResponse? User, BulkUserError? Error)> CreateOneAsync(CreateUserCommand item, int index, CancellationToken ct)
    {
        if (item is null)
            return (null, new BulkUserError(index, null, "El usuario esta vacio"));

        var validation = await _validator.ValidateAsync(item, ct);
        if (!validation.IsValid)
        {
            var mensajes = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
            return (null, new BulkUserError(index, item.Email, mensajes));
        }

        var email = item.Email.Trim().ToLowerInvariant();

        try
        {
            // un DbContext por operacion, no se puede compartir entre tareas concurrentes
            await using var db = await _factory.CreateDbContextAsync(ct);

            var exists = await db.Users.AnyAsync(u => u.Email == email, ct);
            if (exists)
                return (null, new BulkUserError(index, item.Email, "Ya existe un usuario con ese email"));

            var user = new User
            {
                Name = item.Name.Trim(),
                Email = email,
                PasswordHash = string.IsNullOrWhiteSpace(item.Password)
                    ? null
                    : BCrypt.Net.BCrypt.HashPassword(item.Password)
            };

            db.Users.Add(user);
            await db.SaveChangesAsync(ct);

            return (UserResponse.From(user), null);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: 19 })
        {
            // 19 = constraint. Pasa cuando el mismo email viene repetido en la carga
            // y dos tareas intentan insertarlo al mismo tiempo
            return (null, new BulkUserError(index, item.Email, "Ya existe un usuario con ese email"));
        }
        catch (DbUpdateException)
        {
            return (null, new BulkUserError(index, item.Email, "No se pudo guardar el usuario"));
        }
    }
}
