using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericJwtAuth.StartupServices
{
    public class AzureTableRepo : IAzureTableRepo
    {
        public AzureTableRepo()
        {
            this.Collection = new Dictionary<string, CloudTable>();
        }
        public Dictionary<string, CloudTable> Collection { get; set; }
    }

    public interface IAzureTableRepo
    {
        Dictionary<string, CloudTable> Collection { get; set; }
    }
}
