using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace EwellServer.Data;

/* This is used if database provider does't define
 * IEwellServerDbSchemaMigrator implementation.
 */
public class NullEwellServerDbSchemaMigrator : IEwellServerDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
