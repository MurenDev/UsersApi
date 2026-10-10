using UsersApi.Models;

namespace UsersApi.Application.Addresses;

public record AddressResponse(int Id, int UserId, string Street, string City, string Country, string? ZipCode)
{
    public static AddressResponse From(Address address) =>
        new(address.Id, address.UserId, address.Street, address.City, address.Country, address.ZipCode);
}
