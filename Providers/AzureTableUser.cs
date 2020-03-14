using GenericJwtAuth.Config;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UltraMapper;

namespace GenericJwtAuth.Providers
{
    public class AzureTableUser : ITableEntity
    {
        public const string partitionKey = "Users";
        private readonly Mapper mapper;
        public AzureTableUser()
        {
            mapper = new UltraMapper.Mapper(UltramapperConfiguration.Get());
        }
        public virtual Guid Id { get; set; } = Guid.NewGuid();
        public virtual string UserName { get; set; }
        public virtual string Email { get; set; }
        public virtual bool EmailConfirmed { get; set; }
        public virtual String PasswordHash { get; set; }
        public string NormalizedUserName { get; internal set; }
        public string AuthenticationType { get; set; }
        public bool IsAuthenticated { get; set; }
        public string Name { get; set; }

        public string PartitionKey { get => partitionKey; }
        public string RowKey { get => this.NormalizedUserName; }
        public DateTimeOffset Timestamp { get; set; }
        public string ETag { get; set; }
        public string NormalizedEmail { get; internal set; }
        public bool PhoneNumberConfirmed { get; internal set; }
        public bool TwoFactorEnabled { get; internal set; }
        public string PhoneNumber { get; internal set; }
        string ITableEntity.PartitionKey { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string ITableEntity.RowKey { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            if (properties == null) { throw new ArgumentNullException(nameof(properties)); }
            if (operationContext == null) { throw new ArgumentNullException(nameof(operationContext)); }

            AzureTableUser customEntity = TableEntity.ConvertBack<AzureTableUser>(properties, operationContext);
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
