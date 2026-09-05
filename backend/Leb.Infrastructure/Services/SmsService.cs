using System;
using System.Threading.Tasks;
using Leb.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Leb.Infrastructure.Services
{
    public class SmsService : ISmsService
    {
        private readonly ILogger<SmsService> _logger;

        public SmsService(ILogger<SmsService> logger)
        {
            _logger = logger;
        }

        public Task SendSmsAsync(string phoneNumber, string message)
        {
            _logger.LogInformation("MOCK SMS SENT TO: {PhoneNumber}\nMESSAGE: {Message}", phoneNumber, message);
            return Task.CompletedTask;
        }
    }
}
