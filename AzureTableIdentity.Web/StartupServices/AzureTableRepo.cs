using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;

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
