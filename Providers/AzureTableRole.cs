using GenericJwtAuth.Config;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UltraMapper;

namespace GenericJwtAuth.Providers
{
    public class AzureTableRole : ITableEntity
    {
        public AzureTableRole()
        {
            mapper = new UltraMapper.Mapper(UltramapperConfiguration.Get());
        }
        public const string partitionKey = "Roles";
        private readonly Mapper mapper;

        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string PartitionKey { get => partitionKey; set => throw new NotImplementedException(); }
        public string RowKey { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string ETag { get; set; }
        public string NormalizedName { get; internal set; }

        public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            if (properties == null) { throw new ArgumentNullException(nameof(properties)); }
            if (operationContext == null) { throw new ArgumentNullException(nameof(operationContext)); }

            AzureTableRole customEntity = TableEntity.ConvertBack<AzureTableRole>(properties, operationContext);
            // Do the memberwise clone for this object from the returned CustomEntity object
            mapper.Map(this, customEntity);
        }

        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            IDictionary<string, EntityProperty> flattenedProperties = TableEntity.Flatten(this, operationContext);
            //flattenedProperties.Remove("PartitionKey");
            //flattenedProperties.Remove("RowKey");
            //flattenedProperties.Remove("Timestamp");
            //flattenedProperties.Remove("ETag");
            return flattenedProperties;
        }
    }
}
