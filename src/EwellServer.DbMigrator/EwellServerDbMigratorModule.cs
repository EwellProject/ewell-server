using EwellServer.MongoDB;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Modularity;

namespace EwellServer.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(EwellServerMongoDbModule),
    typeof(EwellServerApplicationContractsModule)
    )]
public class EwellServerDbMigratorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpBackgroundJobOptions>(options => options.IsJobExecutionEnabled = false);
    }
}
