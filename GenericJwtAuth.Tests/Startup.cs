using GenericJwtAuth.StartupServices;
using Microsoft.Extensions.Configuration;
using Nivra.AzureOperations;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenericJwtAuth.Tests
{
    public sealed class Startup
    {
        private static readonly Lazy<Startup> lazyStartup = new Lazy<Startup>(() => new Startup());

        public static Startup Instance { get { return lazyStartup.Value; } }

        public static void Initialize() { var tmp = lazyStartup.Value; }

        private Startup()
        {
            Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

            var tables = AzureTables.InitializeAzureTables(Configuration["ConnectionStrings:DefaultConnection"], "Auth");

            foreach (var table in tables)
            {
                AzureTableRepo.Collection.Add(table.Name, table);
            }

            Utility = new Nivra.AzureOperations.Utility(Configuration["ConnectionStrings:DefaultConnection"], "Auth");

            Dict.Add("Email", $"{Helper.GenerateRandomString(9,options: GenerateRandomStringOptions.IncludeAlphabets)}@{Helper.GenerateRandomString(5, GenerateRandomStringOptions.IncludeAlphabets)}.com");

            Dict.Add("Password", $"{Helper.GenerateRandomString(9,GenerateRandomStringOptions.CaseSensitive| GenerateRandomStringOptions.IncludeAlphabets | GenerateRandomStringOptions.IncludeDigits | GenerateRandomStringOptions.IncludeNonAlphaNumericCharacters)}");
        }

        public static IConfiguration Configuration { get; private set; }
        public static IAzureTableRepo AzureTableRepo { get; set; } = new AzureTableRepo();
        public static Utility Utility { get; private set; }

        // Holds all the state information to perform tests.
        public static Dictionary<string, string> Dict { get; set; } = new Dictionary<string, string>();

        //var tables = AzureTables.InitializeAzureTables(Configuration["ConnectionStrings:DefaultConnection"], "Auth");


        public static IConfiguration GetIConfiguration(string outputPath)
        {
            return new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
