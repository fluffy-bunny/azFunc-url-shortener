using System.Threading.Tasks;

namespace dotnetcore.urlshortener.contracts
{
    public interface IUrlShortenerOperationalStore
    {
        Task<ShortUrl> UpsertShortUrlAsync(ShortUrl shortUrl);
        Task<ShortUrl> GetShortUrlAsync(string id);
        Task RemoveShortUrlAsync(string id);
    }

}