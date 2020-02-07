using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using dotnetcore.urlshortener.contracts;
using dotnetcore.urlshortener.Utils;

namespace dotnetcore.urlshortener
{
    public class UrlShortenerService : IUrlShortenerService
    {
        private EventSource<ShortenerEventArgs> _eventSource;
        private IUrlShortenerOperationalStore _urlShortenerOperationalStore;
        private IUrlShortenerExpiryOperationalStore _urlShortenerExpiryOperationalStore;

        void IUrlShortenerEventSource<ShortenerEventArgs>.AddListenter(EventHandler<ShortenerEventArgs> handler)
        {
            _eventSource.AddListenter(handler);
        }

        void IUrlShortenerEventSource<ShortenerEventArgs>.RemoveListenter(EventHandler<ShortenerEventArgs> handler)
        {
            _eventSource.RemoveListenter(handler);
        }

       
        public UrlShortenerService(
            IUrlShortenerOperationalStore urlShortenerOperationalStore,
            IUrlShortenerExpiryOperationalStore urlShortenerExpiryOperationalStore)
        {
            _urlShortenerOperationalStore = urlShortenerOperationalStore;
            _urlShortenerExpiryOperationalStore = urlShortenerExpiryOperationalStore;
            _eventSource = new EventSource<ShortenerEventArgs>();
        }
        public async Task<ShortUrl> UpsertShortUrlAsync(ShortUrl shortUrl)
        {
            Guard.ArgumentNotNull(nameof(shortUrl), shortUrl);
            var expiredRedirectKey = "0000";
            if (!string.IsNullOrEmpty(shortUrl.ExpiredRedirectKey))
            {
               
                Guard.ArguementEvalutate(nameof(shortUrl.ExpiredRedirectKey),
                    (() =>
                    {
                        if (shortUrl.ExpiredRedirectKey.Length != 4)
                        {
                            return (false, "The value must be exactly 4 in length");
                        }

                        return (true, null);
                    }));
                Guard.ArguementEvalutate(nameof(shortUrl.ExpiredRedirectKey),
                    (() =>
                    {
                        Regex r = new Regex("^[a-zA-Z0-9]*$");
                        if (!r.IsMatch(shortUrl.ExpiredRedirectKey))
                        {
                            return (false, "The value must be an alphanumeric");
                        }
                        return (true, null);
                    }));
                expiredRedirectKey = shortUrl.ExpiredRedirectKey;
            }

            var record = await _urlShortenerOperationalStore.UpsertShortUrlAsync(shortUrl);
            record.Id = $"{expiredRedirectKey}{record.Id}";
            record.ExpiredRedirectKey = expiredRedirectKey;
            _eventSource.FireEvent(new ShortenerEventArgs()
            {
                ShortUrl = record,
                EventType = ShortenerEventType.Upsert,
                UtcDateTime = DateTime.UtcNow
            });
            return record;
        }

        public async Task<ShortUrl> GetShortUrlAsync(string id)
        {
            Guard.ArguementEvalutate(nameof(id),
                (() =>
                {
                    if (id.Length <= 4)
                    {
                        return (false, "The value must be greater than 4 in length");
                    }

                    return (true, null);
                }));
            var expireRedirectKey = id.Substring(0, 4);
            var key = id.Substring(4);
            var record = await _urlShortenerOperationalStore.GetShortUrlAsync(key);
            if (record != null)
            {
                _eventSource.FireEvent(new ShortenerEventArgs()
                {
                    ShortUrl = record,
                    EventType = ShortenerEventType.Get,
                    UtcDateTime = DateTime.UtcNow
                });
                return record;
            }

            var expirationRedirectRecord = await _urlShortenerExpiryOperationalStore.GetExpirationRedirectRecordAsync(expireRedirectKey);
            record = new ShortUrl
            {
                LongUrlType = LongUrlType.ExpiryRedirect,
                LongUrl = expirationRedirectRecord.ExpiredRedirectUrl,
                Id = id,
                ExpiredRedirectKey = expireRedirectKey
            };
            _eventSource.FireEvent(new ShortenerEventArgs()
            {
                ShortUrl = record,
                EventType = ShortenerEventType.Expired,
                UtcDateTime = DateTime.UtcNow
            });

            return record;

        }

        public async Task RemoveShortUrlAsync(string id)
        {
            Guard.ArguementEvalutate(nameof(id),
                (() =>
                {
                    if (id.Length <= 4)
                    {
                        return (false, "The value must be greater than 4 in length");
                    }

                    return (true, null);
                }));
           
            var key = id.Substring(4);
            var original = await _urlShortenerOperationalStore.GetShortUrlAsync(key);
            if (original != null)
            {
                _eventSource.FireEvent(new ShortenerEventArgs()
                {
                    ShortUrl = original,
                    EventType = ShortenerEventType.Remove,
                    UtcDateTime = DateTime.UtcNow
                });
                await _urlShortenerOperationalStore.RemoveShortUrlAsync(key);
            }
        }
    }
}
