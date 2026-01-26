namespace Blogsphere.Webapp.Bff.Application.Contracts.TokenExchange
{
    public interface ITokenExchangeService
    {
        Task<string> ExchangeTokenAsync(string originalToken, string scope);
    }
}