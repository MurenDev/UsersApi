namespace UsersApi.Application.Addresses.Commands;

public record UpdateAddressCommand(int Id, string Street, string City, string Country, string? ZipCode);
