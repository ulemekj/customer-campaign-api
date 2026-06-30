namespace CustomerCampaign.API.Exceptions;

public class CampaignException : Exception
{
    public CampaignException(string message) : base(message) { }
}