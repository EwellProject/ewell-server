using AElf.Indexing.Elasticsearch;
using EwellServer.Project;
using EwellServer.Token;
using EwellServer.Users;
using Nest;

namespace EwellServer.Entities;

public class UserProjectInfoIndex : UserProjectInfoBase, IIndexBuild
{
    [Keyword] public string CrowdfundingProjectId { get; set; }
    public CrowdfundingProjectBase CrowdfundingProject { get; set; }
}