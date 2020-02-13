using System;

namespace dotnetcore.urlshortener.contracts
{
    public enum LongUrlType
    {
        Original,
        ExpiryRedirect
    }
    public class ShortUrl
    {
        public string LongUrl { get; set; }
        public string Id { get; set; }
        public string Tenant { get; set; }
        public DateTime? Expiration { get; set; }

    }
}