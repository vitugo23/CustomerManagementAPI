using System.ComponentModel.DataAnnotations;

namespace CustomerManagement.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Phone]
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdated { get; set; }
        public bool IsActive { get; set; } = true;
        
        [MaxLength(50)]
        public string? CustomerType { get; set; } = "Individual"; // Individual, Business, Premium
        
        [MaxLength(500)]
        public string? Notes { get; set; }
        
        // Navigation properties
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
        public ICollection<CustomerOrder> CustomerOrders { get; set; } = new List<CustomerOrder>();
    }
}
