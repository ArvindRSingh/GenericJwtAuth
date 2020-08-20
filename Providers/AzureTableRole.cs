using GenericJwtAuth.Config;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UltraMapper;

namespace GenericJwtAuth.Providers
{
    public class AzureTableRole : TableEntity
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

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            base.ReadEntity(properties, operationContext);
        }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            return base.WriteEntity(operationContext);
        }
    }
}
