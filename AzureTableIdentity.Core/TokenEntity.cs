using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;

namespace AzureTableIdentity
{
    public class TokenEntity : TableEntity
    {
        public TokenEntity()
        {

        }
        public TokenEntity(AzureTableUser user, string token)
        {
            this.User = user;
            this.RowKey = user.UserName;
            this.PartitionKey = "UserTokens";
            this.Token = token;
        }

        public string Provider { get; set; }
        public string Token { get; set; }
        public string ETag { get; set; }
        public AzureTableUser User { get; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public bool IsInactive { get; set; } = false;

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            base.ReadEntity(properties, operationContext);
        }

        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            return base.WriteEntity(operationContext);
        }
    }
}