using System.ComponentModel.DataAnnotations;

namespace CustomerCampaign.API.DTOs;
public class CreateRewardRequest
{
    [Required(ErrorMessage = "Agent ID is required.")]
    public string AgentId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Customer ID is required.")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Customer ID must contain only digits.")]
    public string CustomerId { get; set; } = string.Empty;
}