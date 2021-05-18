using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using OrderSupervisor.Common.Models;

namespace OrderSupervisor.Common.Repositories
{
    public class CloudTableClient : ICloudTableClient
    {
        private readonly string connectionString;
        private readonly string tableName;

        public CloudTableClient(IOptions<StorageAccount> storageAccount)
        {
            connectionString = storageAccount.Value.ConnectionString;
            tableName = storageAccount.Value.TableName;            
        }

        public CloudTable GetCloudTable()
        {
            CloudStorageAccount cloudAccount = CloudStorageAccount.Parse(connectionString);
            var cloudTableclient = cloudAccount.CreateCloudTableClient();
            var table = cloudTableclient.GetTableReference(tableName);
            table.CreateIfNotExistsAsync();

            return table;
        }
    }
}
