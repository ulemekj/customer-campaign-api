using CustomerCampaign.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerCampaign.API.Data;

public class CampaignDbContext : DbContext
{
    public CampaignDbContext(DbContextOptions<CampaignDbContext> options) : base(options)
    {
    }

    public DbSet<CustomerReward> CustomerRewards { get; set; }
}