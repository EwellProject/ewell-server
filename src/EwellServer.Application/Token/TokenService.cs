using System.Threading.Tasks;
using EwellServer.Common;
using EwellServer.Grains.Grain.Token;
using Microsoft.Extensions.Logging;
using Orleans;

namespace EwellServer.Token;

public class TokenService : EwellServerAppService, ITokenService
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<TokenService> _logger;

    public TokenService(IClusterClient clusterClient, ILogger<TokenService> logger)
    {
        _clusterClient = clusterClient;
        _logger = logger;
    }

    public async Task<TokenGrainDto> GetTokenAsync(string chainId, string symbol)
    {
        var grainId = GrainIdHelper.GenerateGrainId(chainId, symbol);

        var tokenGrain = _clusterClient.GetGrain<ITokenGrain>(grainId);

        var grainResultDto = await tokenGrain.GetTokenAsync(new TokenGrainDto
        {
            ChainId = chainId,
            Symbol = symbol
        });
        AssertHelper.IsTrue(grainResultDto.Success, "GetTokenAsync  fail, chainId  {chainId} symbol {symbol}", chainId,
            symbol);
        return grainResultDto.Data;
    }
}