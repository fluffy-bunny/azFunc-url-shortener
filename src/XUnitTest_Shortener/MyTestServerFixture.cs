using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using TestServerFixture;
using UrlShortenerApp;

namespace XUnitTest_Shortener
{
    public class MyTestServerFixture : TestServerFixture<Startup>
    {
        protected override string RelativePathToHostProject => @"../../../../UrlShortenerApp";
        protected override void ConfigureAppConfiguration(WebHostBuilderContext hostingContext, IConfigurationBuilder config)
        {
            var environmentName = hostingContext.HostingEnvironment.EnvironmentName;
            Program.LoadConfigurations(config, environmentName);
            //   config.AddJsonFile($"appsettings.TestServer.json", optional: false);
        }
    }
}
