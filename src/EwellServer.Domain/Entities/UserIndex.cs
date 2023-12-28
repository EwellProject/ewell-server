using System;
using System.Collections.Generic;
using AElf.Indexing.Elasticsearch;
using Nest;
using EwellServer.Entities;

namespace EwellServer.Users.Index;

public class UserIndex : AbstractEntity<Guid>, IIndexBuild
{
    public string AppId { get; set; }
    public Guid UserId { get; set; }
    [Keyword] public string CaHash { get; set; }
    [Nested(Name = "AddressInfos", Enabled = true, IncludeInParent = true, IncludeInRoot = true)]
    public List<UserAddressInfo> AddressInfos { get; set; }
    public long CreateTime { get; set; }
    public long ModificationTime { get; set; }
}