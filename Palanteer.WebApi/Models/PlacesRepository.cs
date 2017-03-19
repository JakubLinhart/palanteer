using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Palanteer.WebApi.Controllers;

namespace Palanteer.WebApi.Models
{
    internal sealed class PlacesRepository
    {
        private CloudTable table;

        public void Connect()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("places");
            table.CreateIfNotExists();
        }


        public Task Insert(Place entity)
        {
            TableOperation insertOperation = TableOperation.Insert(new GenericEntity<Place>(entity, entity.Id));
            return table.ExecuteAsync(insertOperation);
        }

        public Task Delete(string id)
        {
            var operation = TableOperation.Delete(new GenericEntity<Place>(new Place() { Id = id }, id));
            return table.ExecuteAsync(operation);
        }

        public Task Update(Place entity)
        {
            var operation = TableOperation.InsertOrReplace(new GenericEntity<Place>(entity, entity.Id));
            return table.ExecuteAsync(operation);
        }

        public IEnumerable<Place> Get()
        {
            var query =
                new TableQuery<GenericEntity<Place>>().Where(TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.Equal, "0"));

            return table.ExecuteQuery(query).Select(x => x.Entity).ToArray();
        }
    }
}