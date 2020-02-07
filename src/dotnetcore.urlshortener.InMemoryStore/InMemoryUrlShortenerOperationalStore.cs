using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using dotnetcore.urlshortener.contracts;
using dotnetcore.urlshortener.generator.Extensions;
using dotnetcore.urlshortener.Utils;

namespace dotnetcore.urlshortener.InMemoryStore
{
    public class InMemoryUrlShortenerOperationalStore : IUrlShortenerOperationalStore
    {
        private Dictionary<string, ShortUrl> _database;
        private IUrlShortenerAlgorithm _urlShortenerAlgorithm;

        public InMemoryUrlShortenerOperationalStore(IUrlShortenerAlgorithm urlShortenerAlgorithm)
        {
            _urlShortenerAlgorithm = urlShortenerAlgorithm;
            _database = new Dictionary<string, ShortUrl>();
        }
        public async Task<ShortUrl> UpsertShortUrlAsync(ShortUrl shortUrl)
        {
            Guard.ArgumentNotNull(nameof(shortUrl), shortUrl);
            Guard.ArgumentNotNullOrEmpty(nameof(shortUrl.LongUrl), shortUrl.LongUrl);
            Guard.ArgumentNotNull(nameof(shortUrl.Exiration), shortUrl.Exiration);

            shortUrl.LongUrlType = LongUrlType.Original;

            shortUrl.Id = _urlShortenerAlgorithm.GenerateUniqueId();
            _database.Add(shortUrl.Id, shortUrl);
            return shortUrl;
        }

        public async Task<ShortUrl> GetShortUrlAsync(string id)
        {

            Guard.ArgumentNotNullOrEmpty(nameof(id), id);

            if (_database.ContainsKey(id))
            {
                var record = _database[id];
                if (record.Exiration <= DateTime.UtcNow)
                {
                    _database.Remove(id);
                }
                else
                {
                    return record;
                }
            }
            return null;
        }

        public async Task RemoveShortUrlAsync(string id)
        {
            Guard.ArgumentNotNullOrEmpty(nameof(id), id);
            if (_database.ContainsKey(id))
            {
                _database.Remove(id);
            }
        }
    }
}