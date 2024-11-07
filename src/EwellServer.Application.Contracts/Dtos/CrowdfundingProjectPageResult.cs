using System.Collections.Generic;
using EwellServer.Entities;

namespace EwellServer.Dtos;

public class CrowdfundingProjectPageResult : PageResult<CrowdfundingProjectIndex>
{
    public CrowdfundingProjectPageResult(long total, List<CrowdfundingProjectIndex> data) : base(total, data)
    {
    }
}