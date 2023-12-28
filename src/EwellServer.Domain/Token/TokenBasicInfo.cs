using System;
using Nest;

namespace EwellServer.Users.Index;

public class TokenBasicInfo : IToken
{
    public Guid Id { get; set; }
    [Keyword] public string Symbol { get; set; }
    [Keyword] public string Name { get; set; }
    [Keyword] public string Address { get; set; }
    public int Decimals { get; set; }
}