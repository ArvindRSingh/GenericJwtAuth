using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using UltraMapper;

namespace AzureTableIdentity
{
    public class AzureTableRole : TableEntity
    {
        public AzureTableRole()
        {
            base.PartitionKey = "Roles";
            mapper = new UltraMapper.Mapper(UltramapperConfiguration.Get());
        }
        private readonly Mapper mapper;

        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string PartitionKey { get => base.PartitionKey; set => throw new NotImplementedException(); }
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
