namespace UsersApi.Application.Users.Commands;

public record BulkCreateUsersCommand(List<CreateUserCommand> Users);

public record BulkUserError(int Index, string? Email, string Error);

public record BulkCreateUsersResponse(int Total, int Created, int Failed, List<UserResponse> CreatedUsers, List<BulkUserError> Errors);
