using AElf.Indexing.Elasticsearch;


namespace EwellServer.Users.Index;

public class UserRecord : UserRecordBase,IIndexBuild
{
    public CrowdfundingProjectBase CrowdfundingProject { get; set; }
    public TokenBasicInfo ToRaiseToken { get; set; }
    public TokenBasicInfo CrowdFundingIssueToken { get; set; }
}