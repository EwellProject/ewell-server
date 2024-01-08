using System;
using Volo.Abp.EventBus;

namespace EwellServer.Etos;

[EventName("ProjectRegisteredEto")]
public class ProjectRegisteredEto
{
    public string ChainId { get; set; }
    public string ProjectId { get; set; }
    public int TotalPeriod { get; set; }
    public long PeriodDuration { get; set; }
    public DateTime EndTime { get; set; }
}