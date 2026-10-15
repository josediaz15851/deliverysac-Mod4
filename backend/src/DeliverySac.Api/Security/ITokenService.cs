using DeliverySac.Api.Domain;

namespace DeliverySac.Api.Security;

public interface ITokenService
{
    string CreateToken(Usuario usuario);
}
