using System.Threading.Tasks;

namespace dotnetcore.urlshortener.contracts
{
    public interface IUrlShortenerExpiryOperationalStore
    {
        Task<ExpirationRedirectRecord> GetExpirationRedirectRecordAsync(string key);
    }
}