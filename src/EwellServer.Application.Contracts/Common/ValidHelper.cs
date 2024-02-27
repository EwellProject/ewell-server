using System;
using System.Text.RegularExpressions;
using AElf;

namespace EwellServer.Common;

public static class ValidHelper
{
    public const int MaxResultNumber = 200;
    private const char Underline = '_';

    private const string PatternLetters = @"^[A-Za-z]+$"; 
    private const string UppercaseNumericHyphen = @"^[A-Z0-9\-]+$"; 
    public const string NFTSymbolPattern = @"^.+-(?!0+$)[0-9]+$";

    public static bool MatchesPattern(this string input, string pattern)
    {
        return Regex.IsMatch(input, pattern);
    }

    public static bool MatchesChainId(this string chainId)
    {
        return chainId.MatchesPattern(PatternLetters);
    }    

    public static bool MatchesNftSymbol(this string symbol)
    {
        return symbol.Length != 0 &&
               Regex.IsMatch(symbol, NFTSymbolPattern);    }
    
    public static bool MatchesAddress(this string address)
    {
        try
        {
            if (address.IndexOf(Underline) > -1)
            {
                var parts = address.Split(Underline);
                address = parts[1];
            }
            return Base58CheckEncoding.Verify(address);
        }
        catch (Exception)
        {
            return false;
        }
    }
}