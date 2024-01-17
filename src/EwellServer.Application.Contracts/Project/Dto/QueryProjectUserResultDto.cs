using System;
using System.Collections.Generic;

namespace EwellServer.Project.Dto;

public class QueryProjectUserResultDto
{
    public long TotalCount { get; set; } = 0;
    public long TotalAmount { get; set; } = 0;
    public long TotalUser { get; set; } = 0;
    public List<ProjectUserDto> Users { get; set; } = new();
}

public class ProjectUserDto
{
    public string Address { get; set; } 
    public long InvestAmount { get; set; } 
    public DateTime CreateTime { get; set; } 
}