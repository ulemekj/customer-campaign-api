using CustomerCampaign.API.Data;
using CustomerCampaign.API.DTOs;
using CustomerCampaign.API.Exceptions;
using CustomerCampaign.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CustomerCampaign.API.Services;

public class RewardsService : IRewardsService
{
    private readonly CampaignDbContext _context;
    private readonly IPersonService _personService;

    // For demonstration purposes, the start date is hardcoded here
    private static readonly DateTime CampaignStartDate = new DateTime(2026, 6, 25, 0, 0, 0, DateTimeKind.Utc);

    public RewardsService(CampaignDbContext context, IPersonService personService)
    {
        _context = context;
        _personService = personService;
    }

    public async Task<RewardResponse> CreateRewardAsync(CreateRewardRequest request)
    {
        var now = DateTime.UtcNow;

        // Business Rule 1: The campaign is active for a maximum of 7 days from the start date
        if (now > CampaignStartDate.AddDays(7))
        {
            throw new CampaignException("The reward campaign has ended. Submissions are no longer allowed.");
        }

        // Business Rule 2: Limit of maximum 5 rewarded customers per agent per day
        var today = now.Date;
        int dailyCount = await _context.CustomerRewards
            .CountAsync(r => r.AgentId == request.AgentId && r.CreatedAt.Date == today);

        if (dailyCount >= 5)
        {
            throw new CampaignException($"Agent {request.AgentId} has reached the daily limit of 5 rewarded customers.");
        }

        // SOAP Integration: Fetching the customer's full name from the external service via their ID
        string customerName = await _personService.GetPersonNameAsync(request.CustomerId);

        // Mapping DTO to Database Model
        var reward = new CustomerReward
        {
            AgentId = request.AgentId,
            CustomerId = request.CustomerId,
            FullName = customerName,
            CreatedAt = now
        };

        // Persisting data to the database
        _context.CustomerRewards.Add(reward);
        await _context.SaveChangesAsync();

        // Returning the mapped clean response DTO
        return MapToResponse(reward);
    }

    public async Task<int> ProcessCsvReportAsync(IFormFile file)
    {
        // File validation
        if (file == null || file.Length == 0)
            throw new ArgumentException("A valid CSV file is required.");

        int updatedCount = 0;

        using var reader = new StreamReader(file.OpenReadStream());

        // Skipping the CSV file header (CustomerId, PurchaseDate)
        await reader.ReadLineAsync();

        // Reading the CSV file line by line
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Format check: Expected CustomerId, PurchaseDate
            var parts = line.Split(',');
            if (parts.Length < 2) continue;

            string csvCustomerId = parts[0].Trim();
            string rawDate = parts[1].Trim();

            // Using InvariantCulture to ensure stable date parsing across different regional formats
            if (DateTime.TryParse(rawDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime csvPurchaseDate))
            {
                // Finding the reward record in the database that matches the current customer ID
                var reward = await _context.CustomerRewards
                    .FirstOrDefaultAsync(r => r.CustomerId == csvCustomerId);

                if (reward != null)
                {
                    // Merging the CSV data with the existing database record
                    reward.IsPurchaseSuccessful = true;
                    reward.PurchaseDate = csvPurchaseDate;
                    updatedCount++;
                }
            }
        }

        // Saving all merged changes to the database at once
        await _context.SaveChangesAsync();
        return updatedCount;
    }

    public async Task<IEnumerable<RewardResponse>> GetCampaignResultsAsync()
    {
        var results = await _context.CustomerRewards.ToListAsync();
        return results.Select(MapToResponse);
    }

    // Helper method for mapping the entity model to the response DTO
    private static RewardResponse MapToResponse(CustomerReward model)
    {
        return new RewardResponse
        {
            Id = model.Id,
            AgentId = model.AgentId,
            CustomerId = model.CustomerId,
            FullName = model.FullName,
            CreatedAt = model.CreatedAt,
            IsPurchaseSuccessful = model.IsPurchaseSuccessful,
            PurchaseDate = model.PurchaseDate
        };
    }
}