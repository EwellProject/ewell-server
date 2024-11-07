using System.Collections.Generic;

namespace EwellServer.EntityEventHandler.Core.Background.Options;

public class EwellOption
{
    public bool IsReleaseAuto { get; set; }
    public List<EwellInfo> EwellInfoList { get; set; }
    public int CheckTransactionInterval { get; set; } = 30;

}

public class EwellInfo
{
    public string ChainName { get; set; }
    public string AdminKey { get; set; }
    public string EwellAddress { get; set; }
}