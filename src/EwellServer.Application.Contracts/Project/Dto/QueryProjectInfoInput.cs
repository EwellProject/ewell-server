using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EwellServer.Common;
using Volo.Abp.Application.Dtos;

namespace EwellServer.Project.Dto;

public class QueryProjectInfoInput : PagedResultRequestDto, IValidatableObject
{
    [Required] public string ChainId { get; set; }

    public string ProjectId { get; set; }

    public ProjectStatus Status { get; set; }

    public List<ProjectType> Types { get; set; }

    public static string Sorting { get; set; }

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
}