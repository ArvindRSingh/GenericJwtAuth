using GenericJwtAuth.Config;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UltraMapper;

namespace GenericJwtAuth.Providers
{
    public class AzureTableUser : TableEntity
    {
        public const string partitionKey = "Users";
        private readonly Mapper mapper;
        public AzureTableUser()
        {
            mapper = new UltraMapper.Mapper(UltramapperConfiguration.Get());
        }
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        private string _normalizedUserName;
        public string NormalizedUserName
        {
            get => _normalizedUserName == null ? NormalizedEmail : _normalizedUserName;
            set { this._normalizedUserName = value; }
        }
        public string AuthenticationType { get; set; }
        public bool IsAuthenticated { get; set; }
        public string Name { get; set; }

        public string PartitionKey { get => partitionKey; set { } }
        public string RowKey { get => this.NormalizedUserName; set { this.NormalizedUserName = value; } }
        public DateTimeOffset Timestamp { get; set; }
        public string ETag { get; set; }
        private string _normalizedEmail;
        public string NormalizedEmail
        {
            get => this._normalizedEmail == null ? this.Email == null ? this.UserName : this.Email.ToLower() : this._normalizedEmail;
            set { this._normalizedEmail = this.Email = this.UserName = value; }
        }
        public bool PhoneNumberConfirmed { get; internal set; }
        public bool TwoFactorEnabled { get; internal set; }
        public string PhoneNumber { get; internal set; }
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
