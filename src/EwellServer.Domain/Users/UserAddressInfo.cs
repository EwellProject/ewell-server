using Nest;

namespace EwellServer.Users;

public class UserAddressInfo
{
    [Keyword] public string ChainId { get; set; }
    [Keyword] public string Address { get; set; }
}