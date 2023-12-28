using Nest;

namespace EwellServer.Users.Index;

public class UserProjectInfoBase
{
    [Keyword] public string User { get; set; }
    public long InvestAmount { get; set; }
    public long ToClaimAmount { get; set; }
    public long ActualClaimAmount { get; set; }
}