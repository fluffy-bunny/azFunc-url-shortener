using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CosmosDB.Simple.Store.Configuration;
using CosmosDB.Simple.Store.Extensions;
using dotnetcore.urlshortener.contracts;
using dotnetcore.urlshortener.contracts.Models;
using dotnetcore.urlshortener.CosmosDBStore.Extensions;
using dotnetcore.urlshortener.Extensions;
using dotnetcore.urlshortener.generator.Extensions;
using dotnetcore.urlshortener.InMemoryStore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace webApp_urlshortener
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        private IWebHostEnvironment _env;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddUrlShortenerService();
            services.AddGuidUrlShortenerAlgorithm();

            // services.AddInMemoryUrlShortenerOperationalStore();
            services.AddCosmosDBUrlShortenerOperationalStore();

            // wellknown CosmosDB emulator for local development
            string primaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
            string cosmosEndpointUri = "https://localhost:8081";
            if (!_env.IsDevelopment())
            {
                // this sould be keyVault references in your application settings on azure
                primaryKey = Environment.GetEnvironmentVariable("azFunc-shorturl-cosmos-primarykey");
                cosmosEndpointUri = Environment.GetEnvironmentVariable("azFunc-shorturl-cosmos-uri");
            }

            services.AddSimpleItemStore<ShortUrlCosmosDocument>(options =>
            {
                options.EndPointUrl = cosmosEndpointUri;
                options.PrimaryKey = primaryKey;
                options.DatabaseName = "ShortUrlDatabase";
                options.Collection = new Collection()
                {
                    CollectionName = "ShortUrl",
                    ReserveUnits = 400
                };

            });
            services.AddSimpleItemStore<ExpiredShortUrlCosmosDocument>(options =>
            {
                options.EndPointUrl = cosmosEndpointUri;
                options.PrimaryKey = primaryKey;
                options.DatabaseName = "ShortUrlDatabase";
                options.Collection = new Collection()
                {
                    CollectionName = "ExpiredShortUrl",
                    ReserveUnits = 400
                };

            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            IExpiredUrlShortenerOperationalStore expiredUrlShortenerOperationalStore)
        {
            var section = Configuration.GetSection("InMemoryUrlShortenerExpiryOperationalStore");
            var model = new InMemoryUrlShortenerConfigurationModel();

            section.Bind(model);

            foreach (var record in model.Records)
            {
                var su = expiredUrlShortenerOperationalStore.UpsertShortUrlAsync(new ShortUrl
                {
                    Id = record.ExpiredRedirectKey,
                    LongUrl = record.ExpiredRedirectUrl,
                    Expiration = DateTime.UtcNow.AddYears(10)
                }).GetAwaiter().GetResult();

            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
