namespace OrbintSoft.Yauaa.WebSample.AspNetCore.Services
{
    public interface IUserAgentMapper
    {
        IUserAgentModel Enrich(string userAgent);
        string GetUserAgentString(IUserAgentModel record);
    }
}