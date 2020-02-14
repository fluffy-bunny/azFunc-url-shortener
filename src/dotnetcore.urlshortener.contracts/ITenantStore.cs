using dotnetcore.urlshortener.contracts.Models;
using IdentityModel.Client;
using System.Threading.Tasks;

namespace dotnetcore.urlshortener.contracts
{
    public interface ITenant
    {
        Task<TokenResponse> GetAccessTokenAsync();
        Task<TokenResponse> RefreshAccessTokenAsync();

    }
    public interface ITenantStore
    {
        Task<ITenant> GetTenantAsync(string tenant);

    }
}
