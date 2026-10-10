namespace UsersApi.Application.Addresses.Commands;

public record CreateAddressCommand(int UserId, string Street, string City, string Country, string? ZipCode);
