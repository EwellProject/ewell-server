using EwellServer.Entities;
using Nest;

namespace EwellServer.Users;

public class UserProjectInfoBase : AbstractEntity<string>
{
    [Keyword]
    public string ChainId { get; set; }
    
    public long BlockHeight { get; set; }

    [Keyword] public string User { get; set; }
    public long InvestAmount { get; set; }
    public long ToClaimAmount { get; set; }
    public long ActualClaimAmount { get; set; }
}