using Microsoft.Extensions.DependencyInjection;
using EwellServer.Grains;
using EwellServer.Project;
using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace EwellServer;

[DependsOn(
    typeof(EwellServerDomainModule),
    typeof(AbpAccountApplicationModule),
    typeof(EwellServerApplicationContractsModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(EwellServerGrainsModule),
    typeof(AbpSettingManagementApplicationModule)
)]
public class EwellServerApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<EwellServerApplicationModule>(); });
        context.Services.AddTransient<IScheduleSyncDataService, ProjectInfoSyncDataService>();
        context.Services.AddTransient<IScheduleSyncDataService, UserProjectInfoSyncDataService>();
        context.Services.AddTransient<IScheduleSyncDataService, UserRecordSyncDataService>();
        context.Services.AddTransient<IScheduleSyncDataService, WhitelistSyncDataService>();
        context.Services.AddHttpClient();
    }
    
}
