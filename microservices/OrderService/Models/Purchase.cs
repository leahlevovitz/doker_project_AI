using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderService.Models;

[Table("Purchases")]
public class Purchase
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int GiftId { get; set; }

    [Required]
    public int UserId { get; set; }

    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

    public bool IsDraft { get; set; } = true;

    // Saga status: Pending | Confirmed | Cancelled
    public string Status { get; set; } = "Pending";
}
