using System;
using System.Threading.Tasks;
using Leb.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Leb.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string toEmail, string subject, string body)
        {
            _logger.LogInformation("MOCK EMAIL SENT TO: {ToEmail}\nSUBJECT: {Subject}\nBODY:\n{Body}", toEmail, subject, body);
            return Task.CompletedTask;
        }
    }
}
