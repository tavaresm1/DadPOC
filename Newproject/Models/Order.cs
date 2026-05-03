namespace Newproject.Models;

public class Order
{
    public int Id { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public string Status { get; set; } = "Pending";

    public int CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;
}
