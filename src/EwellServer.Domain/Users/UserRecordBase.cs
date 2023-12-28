using System;
using Nest;

namespace EwellServer.Users.Index;

public class UserRecordBase
{
    [Keyword] public string User { get; set; }
    public BehaviorType BehaviorType { get; set; }
    public long ToRaiseTokenAmount { get; set; }
    public long CrowdFundingIssueAmount { get; set; }
    public DateTime DateTime { get; set; }
}