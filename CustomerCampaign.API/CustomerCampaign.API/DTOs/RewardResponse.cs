namespace CustomerCampaign.API.DTOs;
public class RewardResponse
{
    public int Id { get; set; }
    public string AgentId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsPurchaseSuccessful { get; set; }
    public DateTime? PurchaseDate { get; set; }
}