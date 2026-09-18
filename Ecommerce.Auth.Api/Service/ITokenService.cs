using Ecommerce.Auth.Api.Entities;

namespace Ecommerce.Auth.Api.Service
{
    public interface ITokenService
    {
        string GerarToken(Usuario usuario);
    }
}