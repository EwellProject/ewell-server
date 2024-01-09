using System.Collections.Generic;

namespace EwellServer.Common;

public class IdGenerateHelper
{
    public static string GetId(params object[] inputs)
    {
        return inputs.JoinAsString("-");
    }

    public static string GetTokenInfoId(string chainId, string symbol)
    {
        return GetId(chainId, symbol);
    }
}