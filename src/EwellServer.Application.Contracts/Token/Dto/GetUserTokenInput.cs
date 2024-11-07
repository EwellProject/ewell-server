using System.ComponentModel.DataAnnotations;

namespace EwellServer.Token.Dto;

public class GetUserTokenInput
{
    [Required] public string ChainId { get; set; }
}