using System.Threading.Tasks;
using Leb.Core.Entities;

namespace Leb.Core.Interfaces
{
    public interface IJWTService
    {
        string GenerateToken(User user, int? patientId = null, int? staffId = null, int? doctorId = null, int? branchId = null);
        bool ValidateToken(string token, out string email, out string role);
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }

    public interface ISmsService
    {
        Task SendSmsAsync(string phoneNumber, string message);
    }

    public interface IPdfService
    {
        byte[] GenerateReportPdf(Appointment appointment, System.Collections.Generic.IEnumerable<Report> reports);
        byte[] GenerateInvoicePdf(Appointment appointment, Invoice invoice);
    }

    public interface IPasswordHasher
    {
        void HashPassword(string password, out string passwordHash, out string salt);
        bool VerifyPassword(string password, string passwordHash, string salt);
    }
}
