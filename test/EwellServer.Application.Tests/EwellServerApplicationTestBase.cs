using Xunit.Abstractions;

namespace EwellServer;

public abstract partial class EwellServerApplicationTestBase : EwellServerOrleansTestBase<EwellServerApplicationTestModule>
{

    public  readonly ITestOutputHelper Output;
    protected EwellServerApplicationTestBase(ITestOutputHelper output) : base(output)
    {
        Output = output;
    }
}