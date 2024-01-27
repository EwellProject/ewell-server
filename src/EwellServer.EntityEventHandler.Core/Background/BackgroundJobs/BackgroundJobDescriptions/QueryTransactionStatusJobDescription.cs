namespace EwellServer.EntityEventHandler.Core.Background.BackgroundJobs.BackgroundJobDescriptions;

public class QueryTransactionStatusJobDescription
{
    public string ChainName { get; set; }
    public string Id { get; set; }
    public string Operation { get; set; }
    public string TransactionId { get; set; }
    public int CurrentPeriod { get; set; }

    public override string ToString()
    {
        return
            $"ChainName :{ChainName} TransactionId: {TransactionId}, Operation: {Operation}, Hash: {Id}, CurrentPeriod: {CurrentPeriod}";
    }
}