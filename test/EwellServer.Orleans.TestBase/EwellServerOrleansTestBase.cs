using Orleans.TestingHost;
using Volo.Abp.Modularity;
using Xunit.Abstractions;

namespace EwellServer;

public abstract class EwellServerOrleansTestBase<TStartupModule> : 
    EwellServerTestBase<TStartupModule> where TStartupModule : IAbpModule
{

    protected readonly TestCluster Cluster;
    
    public EwellServerOrleansTestBase(ITestOutputHelper output) : base(output)
    {
        Cluster = GetRequiredService<ClusterFixture>().Cluster;
    }
}