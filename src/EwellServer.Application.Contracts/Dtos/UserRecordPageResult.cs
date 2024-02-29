using System.Collections.Generic;
using EwellServer.Entities;

namespace EwellServer.Dtos;

public class UserRecordPageResult : PageResult<UserRecordIndex>
{
    public UserRecordPageResult(long total, List<UserRecordIndex> data) : base(total, data)
    {
    }
}