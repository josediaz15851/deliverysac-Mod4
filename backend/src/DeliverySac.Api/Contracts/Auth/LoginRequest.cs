using System.ComponentModel.DataAnnotations;

namespace DeliverySac.Api.Contracts.Auth;

public class LoginRequest
{
    [Required]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
