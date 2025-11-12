using CustomerManagement.Models.DTOs;

namespace CustomerManagement.Data.Services.Interface
{
    public interface IAddressService
    {
        Task<List<AddressDto>> GetCustomerAddressesAsync(int customerId);
        Task<AddressDto?> GetAddressByIdAsync(int addressId);
        Task<AddressDto> CreateAddressAsync(int customerId, CreateAddressDto createAddressDto);
        Task<AddressDto> UpdateAddressAsync(int addressId, CreateAddressDto updateAddressDto);
        Task<bool> DeleteAddressAsync(int addressId);
        Task<bool> SetPrimaryAddressAsync(int customerId, int addressId);
        }
    }