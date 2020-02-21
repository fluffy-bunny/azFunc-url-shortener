using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CosmosDB.Simple.Store.Configuration;
using CosmosDB.Simple.Store.Extensions;
using dotnetcore.urlshortener.contracts;
using dotnetcore.urlshortener.contracts.Models;
using dotnetcore.urlshortener.CosmosDBStore.Extensions;
using dotnetcore.urlshortener.Extensions;
using dotnetcore.urlshortener.generator.Extensions;
using dotnetcore.urlshortener.InMemoryStore;
using IdentityModel.Client;
using KeyVaultStores.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using webApp_urlshortener.Controllers;
using webApp_urlshortener.Models.jwt_validation;

namespace webApp_urlshortener
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private IWebHostEnvironment _env;
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();

            services.AddControllers();
            var jwt_validate_settings = Configuration["jwt-validate-settings"];
            var authenictation = JsonConvert.DeserializeObject<Authentication>(jwt_validate_settings);
            var tok = authenictation.ToTokenValidationParameters();
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = authenictation.JwtValidation.Authority;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = tok;
                });

            services.AddUrlShortenerService();
            services.AddGuidUrlShortenerAlgorithm();

            // services.AddInMemoryUrlShortenerOperationalStore();
            services.AddCosmosDBUrlShortenerOperationalStore();

            // wellknown CosmosDB emulator for local development

            string primaryKey = Environment.GetEnvironmentVariable("azFunc-shorturl-cosmos-primarykey");
            string cosmosEndpointUri = Environment.GetEnvironmentVariable("azFunc-shorturl-cosmos-uri");
            primaryKey = primaryKey ?? "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
            cosmosEndpointUri = cosmosEndpointUri ?? "https://localhost:8081";

            TenantConfiguration tenantConfiguration = null;
            try
            {
                var creds = Environment.GetEnvironmentVariable("azFunc-shorturl-client-credentials");
                tenantConfiguration = JsonConvert.DeserializeObject<TenantConfiguration>(creds);
            }
            catch (Exception e)
            {

            }
 
            services.AddKeyValutTenantStore(options => {
                options.ExpirationSeconds = 300;
                options.KeyVaultName = "kv-organics";
                options.SecretName = "azFunc-shorturl-client-credentials";
                options.Value = tenantConfiguration; // ok if null.  If it is not null we don't go to key vault at all
            });

            services.AddSimpleItemStore<ShortUrlCosmosDocument>(options =>
            {
                options.EndPointUrl = cosmosEndpointUri;
                options.PrimaryKey = primaryKey;
                options.DatabaseName = "shorturl";
                options.Collection = new Collection()
                {
                    CollectionName = "shorturl",
                    ReserveUnits = 400
                };

            });
            services.AddSimpleItemStore<ExpiredShortUrlCosmosDocument>(options =>
            {
                options.EndPointUrl = cosmosEndpointUri;
                options.PrimaryKey = primaryKey;
                options.DatabaseName = "shorturl";
                options.Collection = new Collection()
                {
                    CollectionName = "expired-shorturl",
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
                    Tenant = record.Tenant,
                    LongUrl = record.ExpiredRedirectUrl,
                    Expiration = DateTime.UtcNow.AddYears(10)
                }).GetAwaiter().GetResult();

            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
