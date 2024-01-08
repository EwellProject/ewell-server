using Volo.Abp.EventBus;

namespace EwellServer.Etos;

[EventName("ProjectCanceledEto")]
public class ProjectCanceledEto
{
    public string CrowdfundingProjectId { get; set; }
    public string ChainId { get; set; }
}