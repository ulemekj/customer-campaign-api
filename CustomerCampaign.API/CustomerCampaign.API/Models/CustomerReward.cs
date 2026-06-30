using System.ComponentModel.DataAnnotations;

namespace CustomerCampaign.API.Models;

public class CustomerReward
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Agent ID is required.")]
    public string AgentId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Customer ID is required.")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Customer ID must contain only digits.")]
    public string CustomerId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsPurchaseSuccessful { get; set; } = false;
    public DateTime? PurchaseDate { get; set; }

    // SOAP service data (FindPerson)
    public string? FullName { get; set; }
    public string? DocumentNumber { get; set; }
}