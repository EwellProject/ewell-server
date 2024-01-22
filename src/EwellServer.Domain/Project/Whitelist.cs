namespace EwellServer.Project;

public class Whitelist
{
    public string ChainId { get; set; }
    
    public long BlockHeight { get; set; }
    public string Id { get; set; }
    public bool IsAvailable { get; set; }
    public string AddressTimeInfo { get; set; }
}