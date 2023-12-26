using System.Threading.Tasks;

namespace EwellServer.Data;

public interface IEwellServerDbSchemaMigrator
{
    Task MigrateAsync();
}
