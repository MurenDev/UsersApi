namespace UsersApi.Security;

public class ApiKeyMiddleware
{
    private const string HeaderName = "X-API-KEY";
    private readonly RequestDelegate _next;
    private readonly string _apiKey;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _apiKey = config["ApiKey"]
            ?? throw new ArgumentNullException("Falta la ApiKey en appsettings.json");
    }

    public async Task InvokeAsync(HttpContext context)
    {
       if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var provided)
            || provided != _apiKey)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "API/Key invalida" });
            return;
        }

        await _next(context);
    }
    }   
