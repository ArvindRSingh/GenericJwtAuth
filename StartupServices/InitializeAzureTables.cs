﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericJwtAuth.StartupServices
{
    public static class AzureTables
    {
        public static IEnumerable<CloudTable> InitializeAzureTables(this IServiceCollection services, string connectionString, params string[] tableNames)
        {
            // Retrieve storage account information from connection string.
            CloudStorageAccount storageAccount;

            try
            {
                storageAccount = CloudStorageAccount.Parse(connectionString);
            }
            catch (FormatException ex)
            {
                throw new FormatException("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.", ex);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.", ex);
            }

            // Create a table client for interacting with the table service
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            // Create a table client for interacting with the table service 
            CloudTable table = null;
            foreach (string tableName in tableNames)
            {
                table = tableClient.GetTableReference(tableName);

                var result = table.CreateIfNotExistsAsync().Result;

                yield return table;
            }


        }
    }
}
