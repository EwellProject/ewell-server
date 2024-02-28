using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EwellServer.Common;

namespace EwellServer.Project.Dto;

public class QueryProjectInfoInput : IValidatableObject
{
    [Required] public string ChainId { get; set; }

    public string ProjectId { get; set; }

    public ProjectStatus Status { get; set; }

    public List<ProjectType> Types { get; set; }
    
    [Range(0, int.MaxValue)]
    public virtual int SkipCount { get; set; }
    
    [Range(1, int.MaxValue)]
    public int MaxResultCount { get; set; } = 1000;
    
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ChainId.IsNullOrEmpty() || !ChainId.MatchesChainId())
        {
            yield return new ValidationResult($"ChainId invalid.");
        }
        
        if (ProjectId.IsNullOrEmpty() && Types.IsNullOrEmpty())
        {
            yield return new ValidationResult($"You must specify the projectId or types.");
        }
    }

    public bool QuerySelf()
    {
        return !Types.IsNullOrEmpty() &&
               (Types.Contains(ProjectType.Created) || Types.Contains(ProjectType.Participate));
    }
}