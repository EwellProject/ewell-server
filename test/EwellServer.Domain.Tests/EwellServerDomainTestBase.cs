using Xunit.Abstractions;

namespace EwellServer;

public abstract class EwellServerDomainTestBase : EwellServerTestBase<EwellServerDomainTestModule>
{
    protected EwellServerDomainTestBase(ITestOutputHelper output) : base(output)
    {
    }
}