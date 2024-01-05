using System.Collections.Generic;
using EwellServer.Entities;

namespace EwellServer.Dtos;

public class UserRecordPageResult : PageResult<UserRecord>
{
    public UserRecordPageResult(long total, List<UserRecord> data) : base(total, data)
    {
    }
}