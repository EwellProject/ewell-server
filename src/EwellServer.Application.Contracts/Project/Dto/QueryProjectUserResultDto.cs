using System;
using System.Collections.Generic;
using EwellServer.Token;

namespace EwellServer.Project.Dto;

public class QueryProjectUserResultDto
{
    public long TotalCount { get; set; } = 0;
    public long TotalAmount { get; set; } = 0;
    public long TotalUser { get; set; } = 0;
    public List<ProjectUserDto> Users { get; set; } = new();

    public string VirtualAddress { get; set; } = "";

    public long CurrentRaisedAmount { get; set; }

    public long CurrentCrowdFundingIssueAmount { get; set; }

    public long TargetRaisedAmount { get; set; }

    public long CrowdFundingIssueAmount { get; set; }

    public TokenBasicInfo ToRaiseToken { get; set; }

    public TokenBasicInfo CrowdFundingIssueToken { get; set; }
}

public class ProjectUserDto
{
    public string Address { get; set; }
    public long InvestAmount { get; set; }
    public DateTime CreateTime { get; set; }
    public string Symbol { get; set; } = "ELF";
    public int Decimals { get; set; } = 8;
}