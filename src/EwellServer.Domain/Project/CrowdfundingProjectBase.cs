using System;
using EwellServer.Entities;
using Nest;

namespace EwellServer.Project;

public class CrowdfundingProjectBase : AbstractEntity<string>
{
    [Keyword]
    public string ChainId { get; set; }
    
    public long BlockHeight { get; set; }
    [Keyword] public override string Id { get; set; }
    [Keyword] public string Creator { get; set; }
    public string CrowdFundingType { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime TokenReleaseTime { get; set; }
}