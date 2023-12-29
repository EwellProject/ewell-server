using System;
using EwellServer.Entities;
using Nest;

namespace EwellServer.Project;

public class CrowdfundingProjectBase : AbstractEntity<Guid>
{
    [Keyword] public string ProjectId { get; set; }
    [Keyword] public string Creator { get; set; }
    public string CrowdFundingType { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}