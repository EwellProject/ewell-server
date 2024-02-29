using AElf.Indexing.Elasticsearch;
using EwellServer.Project;
using EwellServer.Token;

namespace EwellServer.Entities;

public class CrowdfundingProjectIndex : CrowdfundingProjectBasicProperty, IIndexBuild
{
    public TokenBasicInfo ToRaiseToken { get; set; }
    public TokenBasicInfo CrowdFundingIssueToken { get; set; }
}