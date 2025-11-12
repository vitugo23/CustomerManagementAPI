using System.ComponentModel.DataAnnotations;

namespace CustomerManagement.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string OrderNumber { get; set; } = string.Empty;
        
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        
        [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than 0")]
        public decimal TotalAmount { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Processing, Completed, Cancelled
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        // Navigation properties
        public ICollection<CustomerOrder> CustomerOrders { get; set; } = new List<CustomerOrder>();
    }
}
