using Microsoft.Extensions.DependencyInjection;
using EwellServer.Grains;
using EwellServer.MongoDB;
using EwellServer.User;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace EwellServer.Silo;
[DependsOn(typeof(AbpAutofacModule),
    typeof(EwellServerGrainsModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(EwellServerMongoDbModule),
    typeof(EwellServerApplicationModule)
)]
public class EwellServerOrleansSiloModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHostedService<EwellServerHostedService>();
        context.Services.AddTransient<IUserAppService, UserAppService>();
    }
}