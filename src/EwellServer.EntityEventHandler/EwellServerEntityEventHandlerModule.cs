using System;
using AElf.Indexing.Elasticsearch.Options;
using EwellServer.EntityEventHandler.Core;
using EwellServer.EntityEventHandler.Core.Background.BackgroundJobs;
using EwellServer.EntityEventHandler.Core.Background.Options;
using EwellServer.Grains;
using EwellServer.MongoDB;
using EwellServer.Work;
using EwellServer.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Providers.MongoDB.Configuration;
using Volo.Abp;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.EventBus.RabbitMq;
using Volo.Abp.Modularity;
using Volo.Abp.Threading;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.Extensions.Configuration;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.OpenIddict.Tokens;

namespace EwellServer.EntityEventHandler;

[DependsOn(typeof(AbpAutofacModule),
    typeof(EwellServerMongoDbModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(EwellServerEntityEventHandlerCoreModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpEventBusRabbitMqModule),
    typeof(EwellServerWorkerModule)
)]
public class EwellServerEntityEventHandlerModule : AbpModule
{
  public override void ConfigureServices(ServiceConfigurationContext context)
    {
        ConfigureTokenCleanupService();
        var configuration = context.Services.GetConfiguration();
        Configure<WorkerOptions>(configuration);
        Configure<ApiOptions>(configuration.GetSection("Api"));
        Configure<EwellOption>(configuration.GetSection("EwellOption"));
        context.Services.AddHostedService<EwellServerHostedService>();
        context.Services.AddSingleton<IClusterClient>(o =>
        {
            return new ClientBuilder()
                .ConfigureDefaults()
                .UseMongoDBClient(configuration["Orleans:MongoDBClient"])
                .UseMongoDBClustering(options =>
                {
                    options.DatabaseName = configuration["Orleans:DataBase"];;
                    options.Strategy = MongoDBMembershipStrategy.SingleDocument;
                })
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = configuration["Orleans:ClusterId"];
                    options.ServiceId = configuration["Orleans:ServiceId"];
                })
                .ConfigureApplicationParts(parts =>
                    parts.AddApplicationPart(typeof(EwellServerGrainsModule).Assembly).WithReferences())
                //.AddSimpleMessageStreamProvider(AElfIndexerApplicationConsts.MessageStreamName)
                .ConfigureLogging(builder => builder.AddProvider(o.GetService<ILoggerProvider>()))
                .Build();
        });
        ConfigureEsIndexCreation();
        ConfigureGraphQl(context, configuration);
        ConfigureBackgroundJob(configuration);
    }
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var client = context.ServiceProvider.GetRequiredService<IClusterClient>();
        AsyncHelper.RunSync(async ()=> await client.Connect());
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {
        var client = context.ServiceProvider.GetRequiredService<IClusterClient>();
        AsyncHelper.RunSync(client.Close);
    }

    //Create the ElasticSearch Index based on Domain Entity
    private void ConfigureEsIndexCreation()
    {
        Configure<IndexCreateOption>(x => { x.AddModule(typeof(EwellServerDomainModule)); });
    }
    
    //Disable TokenCleanupService
    private void ConfigureTokenCleanupService()
    {
        Configure<TokenCleanupOptions>(x => x.IsCleanupEnabled = false);
    }
    
    private void ConfigureGraphQl(ServiceConfigurationContext context,
        IConfiguration configuration)
    {
        context.Services.AddSingleton(new GraphQLHttpClient(configuration["GraphQL:Configuration"],
            new NewtonsoftJsonSerializer()));
        context.Services.AddScoped<IGraphQLClient>(sp => sp.GetRequiredService<GraphQLHttpClient>());
    }
    
    private void ConfigureBackgroundJob(IConfiguration configuration)
    {
        Configure<AbpBackgroundJobOptions>(options =>
        {
            options.IsJobExecutionEnabled = false;
            var ewellConfiguration = configuration.GetSection("EwellOption");
            var isReleaseAuto = ewellConfiguration.GetSection("IsReleaseAuto").Value;
            if (isReleaseAuto.IsNullOrEmpty())
            {
                return;
            }

            if (!"true".Equals(isReleaseAuto, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            options.IsJobExecutionEnabled = true;
            options.AddJob(typeof(ReleaseProjectTokenJob));
            options.AddJob(typeof(CancelProjectJob));
        });
    }
}