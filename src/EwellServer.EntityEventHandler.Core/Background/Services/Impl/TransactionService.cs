using System.Threading.Tasks;
using AElf;
using AElf.Client.Dto;
using EwellServer.EntityEventHandler.Core.Background.Providers;
using Google.Protobuf;
using Volo.Abp.DependencyInjection;

namespace EwellServer.EntityEventHandler.Core.Background.Services.Impl;

public class TransactionService : ITransactionService, ITransientDependency
{
    private readonly IAElfClientProvider _clientProvider;

    public TransactionService(IAElfClientProvider clientProvider)
    {
        _clientProvider = clientProvider;
    }

    public async Task<string> SendTransactionAsync(string chainName, string privateKey, string toAddress,
        string methodName, IMessage txParam)
    {
        var client = _clientProvider.GetClient(chainName);
        var ownerAddress = client.GetAddressFromPrivateKey(privateKey);
        var transaction = await client.GenerateTransactionAsync(ownerAddress, toAddress, methodName, txParam);
        var signedTransaction = client.SignTransaction(privateKey, transaction);

        var result = await client.SendTransactionAsync(new SendTransactionInput
        {
            RawTransaction = signedTransaction.ToByteArray().ToHex()
        });
        return result.TransactionId;
    }

    public async Task<TransactionResultDto> GetTransactionById(string chainName, string txId)
    {
        var client = _clientProvider.GetClient(chainName);
        return await client.GetTransactionResultAsync(txId);
    }
}