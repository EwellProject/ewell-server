using Microsoft.Extensions.DependencyInjection;
using EwellServer.Grains;
using EwellServer.Grains.Grain.ApplicationHandler;
using EwellServer.MongoDB;
using EwellServer.Options;
using EwellServer.ThirdPart.Exchange;
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
        var configuration = context.Services.GetConfiguration();
        Configure<ChainOptions>(configuration.GetSection("Chains"));
        Configure<ExchangeOptions>(configuration.GetSection("Exchange"));
        Configure<CoinGeckoOptions>(configuration.GetSection("CoinGecko"));
        
        context.Services.AddHostedService<EwellServerHostedService>();
        context.Services.AddTransient<IUserAppService, UserAppService>();
        context.Services.AddTransient<IExchangeProvider, OkxProvider>();
        context.Services.AddTransient<IExchangeProvider, BinanceProvider>();
        context.Services.AddTransient<IExchangeProvider, CoinGeckoProvider>();
    }
}