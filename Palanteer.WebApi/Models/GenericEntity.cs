using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Palanteer.WebApi.Models
{
    public class GenericEntity<T> : ITableEntity
    {
        public GenericEntity()
        {
        }

        public GenericEntity(T entity, string rowKey)
        {
            Entity = entity;
            RowKey = rowKey;
            PartitionKey = "0";
            Timestamp = DateTime.UtcNow;
            ETag = "*";
        }

        public virtual void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            string json = properties["json"].StringValue;
            Entity = JsonConvert.DeserializeObject<T>(json);
        }

        public virtual IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            string json = JsonConvert.SerializeObject(Entity);
            return new Dictionary<string, EntityProperty>()
            {
                { "json", new EntityProperty(json) }
            };
        }

        public string PartitionKey { get; set; }
        public T Entity { get; private set; }
        public string RowKey { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string ETag { get; set; }
    }
}