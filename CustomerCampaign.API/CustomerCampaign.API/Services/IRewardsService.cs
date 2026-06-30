using CustomerCampaign.API.DTOs;

namespace CustomerCampaign.API.Services;

public interface IRewardsService
{
    Task<RewardResponse> CreateRewardAsync(CreateRewardRequest request);
    Task<int> ProcessCsvReportAsync(IFormFile file);
    Task<IEnumerable<RewardResponse>> GetCampaignResultsAsync();
}