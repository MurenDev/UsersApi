using UsersApi.Models;

namespace UsersApi.Application.Users;

public record UserResponse(int Id, string Name, string Email, bool IsActive, DateTime CreatedAt)
{
    public static UserResponse From(User user) =>
        new(user.Id, user.Name, user.Email, user.IsActive, user.CreatedAt);
}