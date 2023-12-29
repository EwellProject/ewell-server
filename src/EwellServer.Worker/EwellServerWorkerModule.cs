using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Modularity;

namespace EwellServer.Worker
{
    [DependsOn(
        typeof(EwellServerApplicationContractsModule),
        typeof(AbpBackgroundWorkersModule)
    )]
    public class EwellServerWorkerModule : AbpModule
    {
        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var backgroundWorkerManger = context.ServiceProvider.GetRequiredService<IBackgroundWorkerManager>();
        }
    }
}