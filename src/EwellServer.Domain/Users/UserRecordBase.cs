using System;
using EwellServer.Entities;
using Nest;

namespace EwellServer.Users;

public class UserRecordBase : AbstractEntity<string>
{
    [Keyword] public string User { get; set; }
    public BehaviorType BehaviorType { get; set; }
    public long ToRaiseTokenAmount { get; set; }
    public long CrowdFundingIssueAmount { get; set; }
    public DateTime DateTime { get; set; }
}