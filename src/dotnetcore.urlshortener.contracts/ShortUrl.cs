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
        public string ExpiredRedirectKey { get; set; }  // 4 digit alphanumeric only
        public string LongUrl { get; set; }
        public string Id { get; set; }
        public DateTime Exiration { get; set; }
        public LongUrlType LongUrlType { get; set; }
    }
}