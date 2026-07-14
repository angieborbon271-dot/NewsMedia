using NewsMedia.Models;

namespace NewsMedia.Api.Services
{
    public interface ITokenService
    {
        string GenerateToken(AppUser user);
    }
}
