using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Globalization;
using UltraMapper;

namespace AzureTableIdentity
{
    public class AzureTableUser : TableEntity
    {
        public const string PARTITIONKEY = "Users";
        private readonly Mapper mapper;
        public AzureTableUser()
        {
            base.PartitionKey = PARTITIONKEY;
            mapper = new UltraMapper.Mapper(UltramapperConfiguration.Get());
        }
        public Guid Id { get; set; }
        private string _username;
        public string UserName
        {
            get { return _username; }
            set
            {
                _username = value;
                this.NormalizedUserName = _username;
            }
        }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        private string _normalizedUserName;
        public string NormalizedUserName
        {
            get => _normalizedUserName == null ? NormalizedEmail : _normalizedUserName;
            set
            {
                this._normalizedUserName = value?.ToLowerInvariant();
                this.RowKey = _normalizedUserName;
            }
        }
        public string AuthenticationType { get; set; }
        public bool IsAuthenticated { get; set; }
        public string Name { get; set; }

        public new string RowKey
        {
            get => base.RowKey;
            set
            {
                base.RowKey = value;
            }
        }
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
