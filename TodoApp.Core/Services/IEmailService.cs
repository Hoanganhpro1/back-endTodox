using System.Threading.Tasks;

namespace TodoApp.Core.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string username);
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}