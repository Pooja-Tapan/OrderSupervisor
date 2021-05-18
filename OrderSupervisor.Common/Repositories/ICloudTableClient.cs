using Microsoft.WindowsAzure.Storage.Table;

namespace OrderSupervisor.Common.Repositories
{
    public interface ICloudTableClient
    {
        CloudTable GetCloudTable();
    }
}
