using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using webApp_urlshortener;

namespace azFunc_urlshortener
{
    public class Ingress
    {
        static ITestServerHttpClient _testServerHttpClient;

        static ITestServerHttpClient FetchTestServerHttpClient(ILogger logger)
        {
            if (_testServerHttpClient == null)
            {
                var hostBuilder = new HostBuilder()
                 .ConfigureWebHost(webHost =>
                 {
                     // Add TestServer
                     webHost.UseTestServer();
                     webHost.UseStartup<Startup>();
                 })
                 .ConfigureLogging((context, loggingBuilder) =>
                 {
                     loggingBuilder.ClearProviders();
                     loggingBuilder.AddProvider(new HostLoggerProvider("me-tenant", logger));
                 });
                // Build and start the IHost
                var host = hostBuilder.StartAsync().GetAwaiter().GetResult();
                _testServerHttpClient = new TestServerHttpClient
                {
                    HttpClient = host.GetTestClient()
                };
            }
            return _testServerHttpClient;
        }

        [FunctionName("Ingress")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", "put", "delete", Route = "{*all}")] HttpRequest request,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var httpRequestMessageFeature = new HttpRequestMessageFeature(request);
            var httpRequestMessage = httpRequestMessageFeature.HttpRequestMessage;

            var path = new PathString(request.Path.Value.ToLower());

            var uriBuilder = new UriBuilder(request.Scheme, request.Host.Host)
            {
                Path = path,
                Query = request.QueryString.Value
            };
            if (request.Host.Port != null)
            {
                uriBuilder.Port = (int)request.Host.Port;
            }

            httpRequestMessage.RequestUri = uriBuilder.Uri;
            httpRequestMessage.Headers.Remove("Host");
            var testServerHttpClient = FetchTestServerHttpClient(log);
            var responseMessage = await testServerHttpClient.HttpClient.SendAsync(httpRequestMessage);
            return responseMessage;

        }
    }
}
