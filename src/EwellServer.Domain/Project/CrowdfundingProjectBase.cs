using System;
using Nest;

namespace EwellServer.Users.Index;

public class CrowdfundingProjectBase
{
    [Keyword] public string ProjectId { get; set; }
    [Keyword] public string Creator { get; set; }
    public string CrowdFundingType { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Guid Id { get; set; }
}