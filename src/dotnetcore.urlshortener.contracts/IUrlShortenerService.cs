﻿using System.Threading.Tasks;

namespace dotnetcore.urlshortener.contracts
{
    public interface IUrlShortenerService : IUrlShortenerEventSource<ShortenerEventArgs>
    {
        Task<ShortUrl> UpsertShortUrlAsync(string expiredKey, ShortUrl shortUrl);
        Task<ShortUrl> GetShortUrlAsync(string id);
        Task RemoveShortUrlAsync(string id);
    }
}
