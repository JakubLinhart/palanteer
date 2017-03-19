using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Palanteer.WebApi.Models
{
    internal sealed class ChatRepository
    {
        private CloudTable table;

        public void Connect()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("chat");
            table.CreateIfNotExists();
        }

        public Task Insert(ChatLine entity)
        {
            TableOperation insertOperation = TableOperation.Insert(new GenericEntity<ChatLine>(entity, IdGenerator.Generate()));
            return table.ExecuteAsync(insertOperation);
            }

        public IEnumerable<ChatLine> GetAfter(DateTime after)
        {
            var query =
                new TableQuery<GenericEntity<ChatLine>>().Where(TableQuery.GenerateFilterConditionForDate("Timestamp",
                    QueryComparisons.GreaterThan, new DateTimeOffset(after)));

            return table.ExecuteQuery(query).Select(x => x.Entity).ToArray();
        }
    }
}