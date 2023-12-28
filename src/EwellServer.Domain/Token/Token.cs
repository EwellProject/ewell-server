using System;

namespace EwellServer.Users.Index;

public class Token : TokenBasicInfo
{
    public Guid ChainId { get; set; }
}