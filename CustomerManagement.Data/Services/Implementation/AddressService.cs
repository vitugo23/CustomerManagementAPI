using CustomerManagement.Data.Interfaces;
using CustomerManagement.Data.Services.Interface;
using CustomerManagement.Models;
using CustomerManagement.Models.DTOs;

namespace CustomerManagement.Data.Services.Implementation
{
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _addressRepository;
        private readonly ICustomerRepository _customerRepository;
        // Add other repositories if needed
        public AddressService(IAddressRepository addressRepository, ICustomerRepository customerRepository)
        {
            _addressRepository = addressRepository;
            _customerRepository = customerRepository;
        }
        // Implement GET all addresses for a customer

        public async Task<List<AddressDto>> GetCustomerAddressesAsync(int customerId)
        {
            var addresses = await _addressRepository.GetByCustomerIdAsync(customerId);
            return addresses.Select(MapToAddressDto).ToList();
        }
        // Implement get address by ID

        public async Task<AddressDto?> GetAddressByIdAsync(int addressId)
        {
            var address = await _addressRepository.GetByIdAsync(addressId);
            return address == null ? null : MapToAddressDto(address);
        }
        // Implement create address

        public async Task<AddressDto> CreateAddressAsync(int customerId, CreateAddressDto createAddressDto)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
                throw new InvalidOperationException("Customer not found");

            var address = new Address
            {
                CustomerId = customerId,
                AddressType = createAddressDto.AddressType,
                Street = createAddressDto.Street,
                City = createAddressDto.City,
                State = createAddressDto.State,
                ZipCode = createAddressDto.ZipCode,
                Country = createAddressDto.Country,
                IsPrimary = createAddressDto.IsPrimary
            };

            // If setting as primary, update other addresses
            if (address.IsPrimary)
            {
                await _addressRepository.SetPrimaryAddressAsync(customerId, 0); // Reset all to false first
            }

            await _addressRepository.AddAsync(address);
            await _addressRepository.SaveChangesAsync();

            return MapToAddressDto(address);
        }
        // Implement update address
        public async Task<AddressDto> UpdateAddressAsync(int addressId, CreateAddressDto updateAddressDto)
        {
            var address = await _addressRepository.GetByIdAsync(addressId);
            if (address == null)
                throw new InvalidOperationException("Address not found");

            address.AddressType = updateAddressDto.AddressType;
            address.Street = updateAddressDto.Street;
            address.City = updateAddressDto.City;
            address.State = updateAddressDto.State;
            address.ZipCode = updateAddressDto.ZipCode;
            address.Country = updateAddressDto.Country;
            address.IsPrimary = updateAddressDto.IsPrimary;

            await _addressRepository.UpdateAsync(address);
            await _addressRepository.SaveChangesAsync();

            return MapToAddressDto(address);
        }
        // Implement delete address

        public async Task<bool> DeleteAddressAsync(int addressId)
        {
            var address = await _addressRepository.GetByIdAsync(addressId);
            if (address == null) return false;

            await _addressRepository.DeleteAsync(addressId);
            await _addressRepository.SaveChangesAsync();
            return true;
        }
        // Implement set primary address
        public async Task<bool> SetPrimaryAddressAsync(int customerId, int addressId)
        {
            await _addressRepository.SetPrimaryAddressAsync(customerId, addressId);
            await _addressRepository.SaveChangesAsync();
            return true;
        }
        // Mapping method

        private AddressDto MapToAddressDto(Address address)
        {
            return new AddressDto
            {
                AddressId = address.AddressId,
                AddressType = address.AddressType,
                FullAddress = $"{address.Street}, {address.City}, {address.State} {address.ZipCode}",
                IsPrimary = address.IsPrimary
            };
        }
    }
}
