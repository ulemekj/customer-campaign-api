namespace CustomerCampaign.API.Services
{
    public interface IPersonService
    {
        // This method receives a person ID and returns the first and last name
        Task<string> GetPersonNameAsync(string id);
    }
}
