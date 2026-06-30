using CustomerCampaign.API.Data;
using CustomerCampaign.API.Models;
using CustomerCampaign.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CustomerCampaign.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RewardsController : ControllerBase
{
    private readonly CampaignDbContext _context;
    private readonly IPersonService _personService;

    public RewardsController(CampaignDbContext context, IPersonService personService)
    {
        _context = context;
        _personService = personService;
    }

    // POST: api/rewards
    [HttpPost]
    public async Task<IActionResult> CreateReward([FromBody] CustomerReward reward)
    {
        // Validation: Data check
        if (reward == null || string.IsNullOrEmpty(reward.AgentId) || string.IsNullOrEmpty(reward.CustomerId))
        {
            return BadRequest("Invalid reward data, Agent ID or Customer ID.");
        }

        // Business rule: Limit of maximum 5 customers per agent in one day
        var today = DateTime.UtcNow.Date;

        int dailyCount = await _context.CustomerRewards
            .CountAsync(r => r.AgentId == reward.AgentId && r.CreatedAt.Date == today);

        if (dailyCount >= 5)
        {
            return BadRequest($"Agent {reward.AgentId} has reached the daily limit of 5 rewarded customers.");
        }

        // SOAP Integration: Fetching the customer name from an external service via ID
        string customerName = await _personService.GetPersonNameAsync(reward.CustomerId);

        reward.FullName = customerName;
        reward.CreatedAt = DateTime.UtcNow; // Ensuring the exact entry date

        // Writing to the database
        _context.CustomerRewards.Add(reward);
        await _context.SaveChangesAsync();

        // Successful response
        return CreatedAtAction(nameof(CreateReward), new { id = reward.Id }, reward);
    }

    // POST: api/rewards/import-csv
    [HttpPost("import-csv")]
    public async Task<IActionResult> ImportCsv(IFormFile file)
    {
        // File validation
        if (file == null || file.Length == 0)
        {
            return BadRequest("A valid CSV file is required.");
        }

        try
        {
            using var reader = new StreamReader(file.OpenReadStream());

            // Skipping the CSV file header (CustomerId,PurchaseDate)
            var header = await reader.ReadLineAsync();

            int updatedCount = 0;

            // Reading CSV line by line
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Format: CustomerId,PurchaseDate
                var parts = line.Split(',');
                if (parts.Length < 2) continue;

                string csvCustomerId = parts[0].Trim();
                string rawDate = parts[1].Trim();

                // Using InvariantCulture to ensure stable date parsing (YMD or MDY formats with dashes/slashes)
                if (DateTime.TryParse(rawDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime csvPurchaseDate))
                {
                    // Finding the reward in the database that matches this customer
                    var reward = await _context.CustomerRewards
                        .FirstOrDefaultAsync(r => r.CustomerId == csvCustomerId);

                    if (reward != null)
                    {
                        // Merging data
                        reward.IsPurchaseSuccessful = true;
                        reward.PurchaseDate = csvPurchaseDate;
                        updatedCount++;
                    }
                }
            }

            // Saving all changes to the database
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"CSV processed successfully. Updated {updatedCount} customers." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Failed to process the requested file: {ex.Message}");
        }
    }

    // GET: api/rewards/results
    [HttpGet("results")]
    public async Task<IActionResult> GetCampaignResults()
    {
        var results = await _context.CustomerRewards.ToListAsync();
        return Ok(results);
    }
}