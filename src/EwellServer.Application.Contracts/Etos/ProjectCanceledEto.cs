using Volo.Abp.EventBus;

namespace EwellServer.Etos;

[EventName("ProjectCanceledEto")]
public class ProjectCanceledEto
{
    public string Id { get; set; }
    public string ChainId { get; set; }
}