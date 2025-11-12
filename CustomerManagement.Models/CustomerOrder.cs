using System.ComponentModel.DataAnnotations;

namespace CustomerManagement.Models
{
    public class CustomerOrder
    {
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        public int OrderId { get; set; }
        
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        
        [MaxLength(50)]
        public string Role { get; set; } = "Primary"; // Primary, Secondary, Billing Contact
        
        // Navigation properties
        public Customer Customer { get; set; } = null!;
        public Order Order { get; set; } = null!;
    }
}