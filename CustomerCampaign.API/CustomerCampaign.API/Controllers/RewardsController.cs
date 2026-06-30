using CustomerCampaign.API.DTOs;
using CustomerCampaign.API.Exceptions;
using CustomerCampaign.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CustomerCampaign.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RewardsController : ControllerBase
{
    private readonly IRewardsService _rewardsService;

    public RewardsController(IRewardsService rewardsService)
    {
        _rewardsService = rewardsService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateReward([FromBody] CreateRewardRequest request)
    {
        try
        {
            var result = await _rewardsService.CreateRewardAsync(request);
            return CreatedAtAction(nameof(CreateReward), new { id = result.Id }, result);
        }
        catch (CampaignException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("import-csv")]
    public async Task<IActionResult> ImportCsv(IFormFile file)
    {
        try
        {
            int updatedCount = await _rewardsService.ProcessCsvReportAsync(file);
            return Ok(new { Message = $"CSV processed successfully. Updated {updatedCount} customers." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Failed to process the requested file: {ex.Message}");
        }
    }

    [HttpGet("results")]
    public async Task<IActionResult> GetCampaignResults()
    {
        var results = await _rewardsService.GetCampaignResultsAsync();
        return Ok(results);
    }
}