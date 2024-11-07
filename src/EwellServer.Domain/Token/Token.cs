using System;

namespace EwellServer.Token;

public class Token : TokenBasicInfo
{
    public Guid ChainId { get; set; }
}