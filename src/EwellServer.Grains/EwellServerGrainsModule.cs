using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace EwellServer.Grains;

[DependsOn(
    typeof(AbpAutoMapperModule),typeof(EwellServerApplicationContractsModule))]
public class EwellServerGrainsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<EwellServerGrainsModule>(); });
    }
}