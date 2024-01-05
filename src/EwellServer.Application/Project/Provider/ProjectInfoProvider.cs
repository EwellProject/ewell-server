using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using EwellServer.Entities;
using EwellServer.Project.Dto;
using Nest;
using Volo.Abp.DependencyInjection;

namespace EwellServer.Project.Provider;

public interface IProjectInfoProvider
{
    public Task<Tuple<long, List<CrowdfundingProjectIndex>>> GetProjectInfosAsync(QueryProjectInfoInput input);
}

public class ProjectInfoProvider : IProjectInfoProvider, ISingletonDependency
{
    private readonly INESTRepository<CrowdfundingProjectIndex, string> _crowdfundingProjectIndexRepository;

    public ProjectInfoProvider(INESTRepository<CrowdfundingProjectIndex, string> crowdfundingProjectIndexRepository)
    {
        _crowdfundingProjectIndexRepository = crowdfundingProjectIndexRepository;
    }

    public async Task<Tuple<long, List<CrowdfundingProjectIndex>>> GetProjectInfosAsync(QueryProjectInfoInput input)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<CrowdfundingProjectIndex>, QueryContainer>>();

        if (!input.ChainId.IsNullOrEmpty())
        {
            mustQuery.Add(q => q.Term(i =>
                i.Field(f => f.ChainId).Value(input.ChainId)));
        }

        QueryContainer Filter(QueryContainerDescriptor<CrowdfundingProjectIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        //sortExp base Sort , like "floorPrice", "itemTotal"
        return await _crowdfundingProjectIndexRepository.GetListAsync(Filter);
    }
}