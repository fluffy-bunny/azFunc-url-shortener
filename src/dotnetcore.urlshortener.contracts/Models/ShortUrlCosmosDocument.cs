using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace dotnetcore.urlshortener.contracts.Models
{
    public static class ShortUrlExtensions
    {
        public static ShortUrlCosmosDocument ToShortUrlCosmosDocument(this ShortUrl shortUrl)
        {
            int? ttl = null;
            if (shortUrl.Exiration != null)
            {
                var diffInSeconds = (int)(((DateTime)shortUrl.Exiration - DateTime.UtcNow).TotalSeconds);
                if (diffInSeconds <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(shortUrl.Exiration));
                }
                ttl = diffInSeconds;
            }



            return new ShortUrlCosmosDocument
            {
                Id = shortUrl.Id,
                ExpiredRedirectKey = shortUrl.ExpiredRedirectKey,
                LongUrl = shortUrl.LongUrl,
                LongUrlType = shortUrl.LongUrlType,
                Expiration = shortUrl.Exiration,
                ttl = ttl
            };
        }
    }

    public class ShortUrlCosmosDocument
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "expiredRedirectKey")]
        public string ExpiredRedirectKey { get; set; }

        [JsonProperty(PropertyName = "longUrl")]
        public string LongUrl { get; set; }

        [JsonProperty(PropertyName = "longUrlType")]
        public LongUrlType LongUrlType { get; set; }

        [JsonProperty(PropertyName = "expiration")]
        public DateTime? Expiration { get; set; }

        [JsonProperty(PropertyName = "ttl", NullValueHandling = NullValueHandling.Ignore)]
        public int? ttl { get; set; }
    }
}

