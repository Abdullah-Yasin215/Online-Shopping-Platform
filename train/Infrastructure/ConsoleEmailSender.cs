using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;

namespace train.Infrastructure
{
    // Dev-only email sender: writes the email to logs/console
    public class ConsoleEmailSender : IEmailSender
    {
        private readonly ILogger<ConsoleEmailSender> _log;
        public ConsoleEmailSender(ILogger<ConsoleEmailSender> log) => _log = log;

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _log.LogInformation("EMAIL → To: {Email} | Subject: {Subject}\n{Body}", email, subject, htmlMessage);
            return Task.CompletedTask;
        }
    }
}
