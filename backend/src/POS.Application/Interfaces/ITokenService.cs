using POS.Domain.Entities;

namespace POS.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
