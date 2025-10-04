using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Services
{
    public interface ITokenService
    {
        string CreateToken(ApplicationUser user, IList<string> roles);
    }
}
