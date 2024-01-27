using System;
using Volo.Abp.BackgroundJobs;

namespace EwellServer.EntityEventHandler.Core.Background.BackgroundJobs.BackgroundJobDescriptions;

[BackgroundJobName("ReleaseProjectTokenJobDescription")]
public class ReleaseProjectTokenJobDescription
{
    public string ChainName { get; set; }
    public string Id { get; set; }
    public int CurrentPeriod { get; set; }
    public int TotalPeriod { get; set; }
    public long PeriodDuration { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public bool IsNeedUnlockLiquidity{ get; set; }

    public override string ToString()
    {
        return
            $"ChainName: {ChainName}, Hash: {Id}, CurrentPeriod: {CurrentPeriod}, TotalPeriod: {TotalPeriod}, PeriodDuration: {PeriodDuration}, StartTime: {StartTime}, IsNeedUnlockLiquidity:{IsNeedUnlockLiquidity}";
    }
}