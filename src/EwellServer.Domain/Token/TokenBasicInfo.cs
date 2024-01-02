using System;
using EwellServer.Entities;
using Nest;

namespace EwellServer.Token;

public class TokenBasicInfo : AbstractEntity<Guid>, IToken
{
    [Keyword] public string Symbol { get; set; }
    [Keyword] public string Name { get; set; }
    [Keyword] public string Address { get; set; }
    public int Decimals { get; set; }
}