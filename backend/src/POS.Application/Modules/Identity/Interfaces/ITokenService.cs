using POS.Domain.Modules.Identity.Entities;

namespace POS.Application.Modules.Identity.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
