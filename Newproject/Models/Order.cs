using System.ComponentModel.DataAnnotations;

namespace Newproject.Models;

public class Order
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    [Required, MaxLength(50)]
    public string Status { get; set; } = "Pending";

    public int CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;
}
