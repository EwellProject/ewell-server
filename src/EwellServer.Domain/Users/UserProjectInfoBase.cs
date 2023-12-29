using System;
using EwellServer.Entities;
using Nest;

namespace EwellServer.Users;

public class UserProjectInfoBase : AbstractEntity<Guid>
{
    [Keyword] public string User { get; set; }
    public long InvestAmount { get; set; }
    public long ToClaimAmount { get; set; }
    public long ActualClaimAmount { get; set; }
}