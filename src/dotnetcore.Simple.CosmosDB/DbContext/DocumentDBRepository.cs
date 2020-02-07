using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CosmosDB.Simple.Store.Abstracts;
using CosmosDB.Simple.Store.Configuration;
using CosmosDB.Simple.Store.Interfaces;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CosmosDB.Simple.Store.DbContext
{
    public class DocumentDBRepository<T> : CosmosDbContextBase, ISimpleItemDbContext<T> where T : class
    {

        private Uri _documentCollectionUri;

        public DocumentDBRepository(
            IOptions<CosmosDbConfiguration> settings,
            ConnectionPolicy connectionPolicy = null,
            ILogger<DocumentDBRepository<T>> logger = null) :
            base(settings, connectionPolicy, logger)
        {
            Guard.ForNullOrDefault(settings.Value, nameof(settings));

            SetupAsync().Wait();
        }
        private async Task SetupAsync()
        {
            _documentCollectionUri =
                UriFactory.CreateDocumentCollectionUri(Database.Id, Configuration.Collection.CollectionName);
            Logger?.LogDebug($"Persisted Grants URI: {_documentCollectionUri}");
            await CreateCollectionIfNotExistsAsync();

        }
        public async Task<T> GetItemAsync(string id)
        {
            try
            {
                Document document = await DocumentClient.ReadDocumentAsync(UriFactory.CreateDocumentUri(Database.Id, Configuration.Collection.CollectionName, id));
                return (T)(dynamic)document;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate)
        {
            IDocumentQuery<T> query = DocumentClient.CreateDocumentQuery<T>(
                    _documentCollectionUri,
                    new FeedOptions { MaxItemCount = -1 })
                .Where(predicate)
                .AsDocumentQuery();

            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }

            return results;
        }

        public async Task<Document> CreateItemAsync(T item)
        {
            return await DocumentClient.CreateDocumentAsync(_documentCollectionUri, item);
        }

        public async Task<Document> UpdateItemAsync(string id, T item)
        {
            return await DocumentClient.ReplaceDocumentAsync(_documentCollectionUri, item);
        }

        public async Task DeleteItemAsync(string id)
        {
            await DocumentClient.DeleteDocumentAsync(_documentCollectionUri);
        }

        private async Task CreateCollectionIfNotExistsAsync()
        {
            try
            {
                await DocumentClient.ReadDocumentCollectionAsync(_documentCollectionUri);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await DocumentClient.CreateDocumentCollectionAsync(
                        DatabaseUri,
                        new DocumentCollection { Id = Configuration.Collection.CollectionName },
                        new RequestOptions { OfferThroughput = Configuration.Collection.ReserveUnits });
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
