using System.Net.Http;

namespace azFunc_urlshortener
{
    public interface ITestServerHttpClient
    {
        HttpClient HttpClient { get; }
    }
}
