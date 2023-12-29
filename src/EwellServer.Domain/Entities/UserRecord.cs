using AElf.Indexing.Elasticsearch;
using EwellServer.Project;
using EwellServer.Token;
using EwellServer.Users;

namespace EwellServer.Entities;

public class UserRecord : UserRecordBase, IIndexBuild
{
    public CrowdfundingProjectBase CrowdfundingProject { get; set; }
    public TokenBasicInfo ToRaiseToken { get; set; }
    public TokenBasicInfo CrowdFundingIssueToken { get; set; }
}