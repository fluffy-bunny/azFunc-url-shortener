using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CosmosDB.Simple.Store.Abstracts;
using CosmosDB.Simple.Store.Configuration;
using CosmosDB.Simple.Store.Interfaces;
using dotnetcore.urlshortener.Utils;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CosmosDB.Simple.Store.DbContext
{
    public class DocumentDBRepository<T> : CosmosDbContextBase<T>, ISimpleItemDbContext<T> where T : class
    {

        private Uri _documentCollectionUri;

        public Uri DocumentCollectionUri => _documentCollectionUri;

        public DocumentDBRepository(
            IOptions<CosmosDbConfiguration<T>> settings,
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
            await CreateContainerIfNotExistsAsync();

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

        public async Task<ItemResponse<T>> UpsertItemV3Async(T item)
        {
            var container = CosmosClient.GetContainer(Database.Id, Configuration.Collection.CollectionName);
            var response = await container.UpsertItemAsync(item);
            return response;
        }

        public async Task<Document> UpsertItemAsync(T item)
        {
            return await DocumentClient.UpsertDocumentAsync(_documentCollectionUri, item);
        }

        public async Task<Document> ReplaceItemAsync(string id, T item)
        {
            return await DocumentClient.ReplaceDocumentAsync(_documentCollectionUri, item);
        }

        public async Task DeleteItemAsync(string id)
        {
            var item = DocumentClient.CreateDocumentQuery(_documentCollectionUri)
                    .Where(d => d.Id == id)
                    .AsEnumerable()
                    .FirstOrDefault();

            await DocumentClient.DeleteDocumentAsync(item.SelfLink);
        }

        private async Task CreateContainerIfNotExistsAsync()
        {
            try
            {
                // Create new container
                var containerProperties = new ContainerProperties
                {
                    Id = Configuration.Collection.CollectionName,
                    PartitionKeyPath = "/id",
                    DefaultTimeToLive = -1
                };

                await DatabaseV3.CreateContainerIfNotExistsAsync(containerProperties);

                await DocumentClient.ReadDocumentCollectionAsync(_documentCollectionUri);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await DocumentClient.CreateDocumentCollectionAsync(
                        DatabaseUri,
                        new DocumentCollection { Id = Configuration.Collection.CollectionName },
                        new Microsoft.Azure.Documents.Client.RequestOptions
                        {
                            OfferThroughput = Configuration.Collection.ReserveUnits,
                        });
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate)
        {
            var items = DocumentClient
                .CreateDocumentQuery<T>(
                DocumentCollectionUri)
                .Where(predicate)
                .AsEnumerable();
            return items;

        }
    }
}
