using System.Collections.Generic;
using System.Text;

namespace EwellServer.EntityEventHandler.Core.Background.BackgroundJobs.BackgroundJobDescriptions;

public class CancelProjectJobDescription
{
    public string ChainName { get; set; }
    public string Id { get; set; }
    public List<string> Users { get; set; } = new ();
    
    public override string ToString()
    {
        var users = new StringBuilder();
        for (var i = 0; i< Users.Count; i ++)
        {
            if (i < Users.Count - 1)
            {
                users.Append($"{Users[i]}, ");
                continue;
            }
            users.Append(Users[i]);
        }
       
        return
            $"ChainName :{ChainName} Hash: {Id}, Users: {users.ToString()}";
    }
}