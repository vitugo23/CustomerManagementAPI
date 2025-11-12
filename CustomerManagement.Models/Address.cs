using System.ComponentModel.DataAnnotations;

namespace CustomerManagement.Models
{
    public class Address
    {
        public int AddressId { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string AddressType { get; set; } = "Home"; // Home, Work, Billing, Shipping
        
        [Required]
        [MaxLength(200)]
        public string Street { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string State { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(20)]
        public string ZipCode { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string Country { get; set; } = "USA";
        
        public bool IsPrimary { get; set; } = false;
        
        // Navigation property
        public Customer Customer { get; set; } = null!;
    }
}