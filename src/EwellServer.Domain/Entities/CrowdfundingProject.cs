using AElf.Indexing.Elasticsearch;

namespace EwellServer.Users.Index;

public class CrowdfundingProject : CrowdfundingProjectBasicProperty, IIndexBuild
{
    public TokenBasicInfo ToRaiseToken { get; set; }
    public TokenBasicInfo CrowdFundingIssueToken { get; set; }
}