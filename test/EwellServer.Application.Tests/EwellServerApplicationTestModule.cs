using Microsoft.Extensions.DependencyInjection;
using Moq;
using EwellServer.EntityEventHandler.Core;
using Volo.Abp.AuditLogging;
using Volo.Abp.AuditLogging.MongoDB;
using Volo.Abp.AutoMapper;
using Volo.Abp.EventBus;
using Volo.Abp.Identity;
using Volo.Abp.Identity.MongoDB;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace EwellServer;

[DependsOn(
    typeof(AbpEventBusModule),
    typeof(EwellServerApplicationModule),
    typeof(EwellServerApplicationContractsModule),
    typeof(EwellServerOrleansTestBaseModule),
    typeof(EwellServerDomainTestModule)
)]
public class EwellServerApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        base.ConfigureServices(context);
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<EwellServerApplicationModule>(); });
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<EwellServerEntityEventHandlerCoreModule>(); });

        context.Services.AddSingleton(new Mock<IMongoDbContextProvider<IAuditLoggingMongoDbContext>>().Object);
        context.Services.AddSingleton<IAuditLogRepository, MongoAuditLogRepository>();
        context.Services.AddSingleton<IIdentityUserRepository, MongoIdentityUserRepository>();
    }
}