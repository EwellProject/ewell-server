using System.ComponentModel.DataAnnotations;

namespace EwellServer.Token.Dto;

public class GetTokenPriceInput
{
    [Required] public string BaseCoin { get; set; }
    [Required] public string QuoteCoin { get; set; }
}