using System.Collections.Generic;
using EwellServer.Entities;
using EwellServer.Project;

namespace EwellServer.Dtos;

public class WhitelistPageResult : PageResult<Whitelist>
{
    public WhitelistPageResult(long total, List<Whitelist> data) : base(total, data)
    {
    }
}