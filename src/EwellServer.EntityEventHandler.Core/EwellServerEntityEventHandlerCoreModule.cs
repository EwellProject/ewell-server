using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace EwellServer.EntityEventHandler.Core
{
    [DependsOn(typeof(AbpAutoMapperModule),
        typeof(EwellServerApplicationModule),
        typeof(EwellServerApplicationContractsModule))]
    public class EwellServerEntityEventHandlerCoreModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpAutoMapperOptions>(options =>
            {
                options.AddMaps<EwellServerEntityEventHandlerCoreModule>();
            });
        }
    }
}