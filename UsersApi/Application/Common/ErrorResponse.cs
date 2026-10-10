namespace UsersApi.Application.Common;

// formato de los errores 404, 409 y 500: { "error": "mensaje" }
public record ErrorResponse(string Error);
