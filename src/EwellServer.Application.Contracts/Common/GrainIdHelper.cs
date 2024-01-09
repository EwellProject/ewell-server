using System.Collections.Generic;

namespace EwellServer.Common;

public static class GrainIdHelper
{
    public static string GenerateGrainId(params object[] ids)
    {
        return ids.JoinAsString("-");
    }
}