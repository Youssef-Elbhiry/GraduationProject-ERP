using ERP.Application.Auth.Dtos;

namespace ERP.Application.Auth;

public interface IAuthService
{
    Task<AuthResponse> CreateUserAsync(RegisterUserDto userDto);
    Task<AuthResponse> GetTokenAsync(LoginUserDto loginuserdto);
    Task<AuthResponse> RefreshTokenAsync(string token);
    Task<bool> RevokeTokenAsync(string token);
}
