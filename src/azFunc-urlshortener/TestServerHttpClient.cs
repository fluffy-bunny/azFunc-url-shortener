using System.Net.Http;

namespace azFunc_urlshortener
{
    class TestServerHttpClient : ITestServerHttpClient
    {
        public HttpClient HttpClient { get; set; }
    }
}
