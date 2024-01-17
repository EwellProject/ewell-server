using System.Collections.Generic;
using EwellServer.Common;
using EwellServer.Entities;

namespace EwellServer.Project.Index;

public class IndexerUserProjectInfoSync : IndexerCommonResult<IndexerUserProjectInfoSync>
{
    public long TotalRecordCount { get; set; }
    
    public List<UserProjectInfoIndex> DataList { get; set; }
}