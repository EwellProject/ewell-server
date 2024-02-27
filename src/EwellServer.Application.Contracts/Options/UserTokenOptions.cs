using System.Collections.Generic;
using EwellServer.Common;

namespace EwellServer.Options;

public class UserTokenOptions
{
    //to filter Nft 
    public bool FilterNft { get; set; } = true;

    //if FilterNft is true, only support NftSet
    public HashSet<string> NftSet { get; set; } = new ();
    
    public bool ToFilterNft(string symbol)
    {
        if (!FilterNft)
        {
            return true;
        }
        
        //check NftSet whether contain nft
        if (symbol.MatchesNftSymbol() && !NftSet.Contains(symbol))
        {
            return false;
        }

        return true;
    }
}