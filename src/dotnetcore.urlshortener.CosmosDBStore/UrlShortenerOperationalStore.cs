using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CosmosDB.Simple.Store.Interfaces;
using dotnetcore.urlshortener.contracts;
using dotnetcore.urlshortener.contracts.Models;
using dotnetcore.urlshortener.generator.Extensions;
using dotnetcore.urlshortener.Utils;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace dotnetcore.urlshortener.CosmosDBStore
{
    public class UrlShortenerOperationalStore :
        UrlShortenerOperationalStoreAbstract<ShortUrlCosmosDocument>, IUrlShortenerOperationalStore
    {
        public UrlShortenerOperationalStore(
            IUrlShortenerAlgorithm urlShortenerAlgorithm,
            ISimpleItemDbContext<ShortUrlCosmosDocument> simpleItemDbContext) :
            base(urlShortenerAlgorithm, simpleItemDbContext)
        {
        }

        public override ShortUrlCosmosDocument CreateDocument(ShortUrlCosmosDocumentBase shortUrlCosmosDocumentBase)
        {
            var document = new ShortUrlCosmosDocument(shortUrlCosmosDocumentBase);
            return document;
        }
    }
    public class ExpiredUrlShortenerOperationalStore :
        UrlShortenerOperationalStoreAbstract<ExpiredShortUrlCosmosDocument>, IExpiredUrlShortenerOperationalStore
    {
        public ExpiredUrlShortenerOperationalStore(
            IUrlShortenerAlgorithm urlShortenerAlgorithm,
            ISimpleItemDbContext<ExpiredShortUrlCosmosDocument> simpleItemDbContext) :
            base(urlShortenerAlgorithm, simpleItemDbContext)
        {
        }

        public override ExpiredShortUrlCosmosDocument CreateDocument(ShortUrlCosmosDocumentBase shortUrlCosmosDocumentBase)
        {
            var document = new ExpiredShortUrlCosmosDocument(shortUrlCosmosDocumentBase);
            return document;
        }
    }
    public abstract class UrlShortenerOperationalStoreAbstract<T> : IUrlShortenerOperationalStoreBase
        where T : ShortUrlCosmosDocumentBase
    {
        private IUrlShortenerAlgorithm _urlShortenerAlgorithm;
        private ISimpleItemDbContext<T> _simpleItemDbContext;

        public UrlShortenerOperationalStoreAbstract(
            IUrlShortenerAlgorithm urlShortenerAlgorithm,
            ISimpleItemDbContext<T> simpleItemDbContext)
        {
            _urlShortenerAlgorithm = urlShortenerAlgorithm;
            _simpleItemDbContext = simpleItemDbContext;

        }
        public async Task<(HttpStatusCode, ShortUrl)> UpsertShortUrlAsync(ShortUrl shortUrl)
        {
            Guard.ArgumentNotNull(nameof(shortUrl), shortUrl);
            Guard.ArgumentNotNullOrEmpty(nameof(shortUrl.LongUrl), shortUrl.LongUrl);
            Guard.ArgumentNotNull(nameof(shortUrl.Expiration), shortUrl.Expiration);
            if (string.IsNullOrWhiteSpace(shortUrl.Id))
            {
                shortUrl.Id = _urlShortenerAlgorithm.GenerateUniqueId();
            }

            var document = CreateDocument(shortUrl.ToShortUrlCosmosDocument());

            // var document = new ShortUrlCosmosDocument(shortUrl.ToShortUrlCosmosDocument());
            var response = await _simpleItemDbContext.UpsertItemV3Async(document);
            return (response.StatusCode,shortUrl);
        }

        public abstract T CreateDocument(ShortUrlCosmosDocumentBase shortUrlCosmosDocumentBase);

        public async Task<ShortUrl> GetShortUrlAsync(string id)
        {

            Guard.ArgumentNotNullOrEmpty(nameof(id), id);
            var item = await _simpleItemDbContext.GetItemAsync(id);
            return item.ToShortUrl();
        }

        public async Task RemoveShortUrlAsync(string id)
        {
            Guard.ArgumentNotNullOrEmpty(nameof(id), id);
            await _simpleItemDbContext.DeleteItemAsync(id);
        }

        public async Task<ShortUrl> GetShortUrlAsync(string id, string tenant)
        {
            var items = await _simpleItemDbContext.GetItemsAsync(so => so.Tenant == tenant && so.Id == id);
            var item = items.FirstOrDefault();
            if (item == null)
            {
                return null;
            }
            return item.ToShortUrl();
        }

        public async Task RemoveShortUrlAsync(string id, string tenant)
        {
            var shortUrl = await GetShortUrlAsync(id);
            if (shortUrl != null && shortUrl.Tenant == tenant)
            {
                await RemoveShortUrlAsync(id);
            }
        }
    }
}