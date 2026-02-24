using System.Threading.Tasks;
using TodoApp.Core.DTOs;

namespace TodoApp.Core.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeTokenAsync(string refreshToken);
        Task<bool> ForgotPasswordAsync(string email); // ✅ Thêm
        Task<bool> ResetPasswordAsync(string token, string newPassword); // ✅ Thêm
    }
}