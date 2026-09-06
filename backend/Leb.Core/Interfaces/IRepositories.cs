using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Leb.Core.Entities;
using Leb.Core.DTOs;

namespace Leb.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<int> CreateUserAsync(User user);
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int userId);
        Task UpdatePasswordAsync(int userId, string passwordHash, string salt);
        Task<bool> VerifyEmailAsync(string token);
        Task<bool> SetResetTokenAsync(string email, string token, DateTime expiry);
        Task<User?> GetUserByResetTokenAsync(string token);
        Task<IEnumerable<User>> GetUsersWithRolesAsync();
    }

    public interface IPatientRepository
    {
        Task<int> CreatePatientAsync(Patient patient);
        Task<Patient?> GetPatientByIdAsync(int patientId);
        Task<Patient?> GetPatientByUserIdAsync(int userId);
        Task UpdatePatientAsync(Patient patient);
        Task DeletePatientAsync(int patientId);
        Task<IEnumerable<Patient>> GetPatientsListAsync();
    }

    public interface IDoctorRepository
    {
        Task<int> CreateDoctorAsync(Doctor doctor);
        Task<Doctor?> GetDoctorByIdAsync(int doctorId);
        Task UpdateDoctorAsync(Doctor doctor);
        Task DeleteDoctorAsync(int doctorId);
        Task<IEnumerable<Doctor>> GetDoctorsListAsync();
    }

    public interface IStaffRepository
    {
        Task<int> CreateStaffAsync(Staff staff);
        Task<Staff?> GetStaffByIdAsync(int staffId);
        Task UpdateStaffAsync(Staff staff);
        Task DeleteStaffAsync(int staffId);
        Task<IEnumerable<Staff>> GetStaffListAsync();
    }

    public interface IBranchRepository
    {
        Task<int> CreateBranchAsync(Branch branch);
        Task<Branch?> GetBranchByIdAsync(int branchId);
        Task UpdateBranchAsync(Branch branch);
        Task DeleteBranchAsync(int branchId);
        Task<IEnumerable<Branch>> GetAllBranchesAsync();
        Task UpdateBranchLetterheadAsync(int branchId, string? gujaratiTitle, string? doctor1Name, string? doctor1Degree, string? doctor2Name, string? doctor2Degree, string? timingInfo, string? letterheadImagePath);
    }

    public interface ITimeSlotRepository
    {
        Task<int> CreateTimeSlotAsync(TimeSlot slot);
        Task<IEnumerable<TimeSlot>> GetSlotsByBranchAsync(int branchId);
        Task UpdateTimeSlotAsync(TimeSlot slot);
        Task DeleteTimeSlotAsync(int slotId);
    }

    public interface IHolidayRepository
    {
        Task<int> CreateHolidayAsync(Holiday holiday);
        Task<IEnumerable<Holiday>> GetHolidaysByBranchAsync(int branchId);
        Task DeleteHolidayAsync(int holidayId);
    }

    public interface ITestCategoryRepository
    {
        Task<int> CreateCategoryAsync(TestCategory category);
        Task<TestCategory?> GetCategoryByIdAsync(int categoryId);
        Task UpdateCategoryAsync(TestCategory category);
        Task DeleteCategoryAsync(int categoryId);
        Task<IEnumerable<TestCategory>> GetAllCategoriesAsync();
    }

    public interface ITestRepository
    {
        Task<int> CreateTestAsync(Test test);
        Task<Test?> GetTestByIdAsync(int testId);
        Task UpdateTestAsync(Test test);
        Task DeleteTestAsync(int testId);
        Task<IEnumerable<Test>> GetAllTestsAsync();
    }

    public interface IPackageRepository
    {
        Task<int> CreatePackageAsync(Package package);
        Task<Package?> GetPackageByIdAsync(int packageId);
        Task UpdatePackageAsync(Package package);
        Task DeletePackageAsync(int packageId);
        Task<IEnumerable<Package>> GetAllPackagesAsync();
        Task AddTestToPackageAsync(int packageId, int testId);
        Task RemoveTestFromPackageAsync(int packageId, int testId);
        Task<IEnumerable<Test>> GetPackageTestsAsync(int packageId);
    }

    public interface IAppointmentRepository
    {
        Task<int> CreateAppointmentAsync(Appointment appointment, List<int> testIds, List<int> packageIds);
        Task<Appointment?> GetAppointmentByIdAsync(int appointmentId);
        Task<IEnumerable<Appointment>> GetAppointmentsByPatientAsync(int patientId);
        Task<IEnumerable<Appointment>> GetAppointmentsByBranchAsync(int branchId);
        Task<IEnumerable<Appointment>> GetAllAppointmentsAsync();
        Task UpdateAppointmentStatusAsync(int appointmentId, string status, int? staffId = null);
        Task UpdateAppointmentPaymentStatusAsync(int appointmentId, string paymentStatus, decimal paidAmount);
        Task RescheduleAppointmentAsync(int appointmentId, DateTime date, int slotId);
        Task CancelAppointmentAsync(int appointmentId);
        Task LogPrintAuditAsync(int appointmentId, int staffId);
    }

    public interface IReportRepository
    {
        Task<int> CreateReportAsync(Report report);
        Task<Report?> GetReportByIdAsync(int reportId);
        Task<IEnumerable<Report>> GetReportsByAppointmentAsync(int appointmentId);
        Task UpdateReportOutsourceAsync(int reportId, bool isOutsourced, string? externalLabName, string? externalBarcode);
    }

    public interface IPaymentRepository
    {
        Task<int> CreatePaymentAsync(Payment payment);
        Task<IEnumerable<Payment>> GetPaymentsByAppointmentAsync(int appointmentId);
        Task<IEnumerable<Payment>> GetAllPaymentsAsync();
    }

    public interface IInvoiceRepository
    {
        Task<int> CreateInvoiceAsync(Invoice invoice);
        Task<Invoice?> GetInvoiceByAppointmentAsync(int appointmentId);
    }

    public interface INotificationRepository
    {
        Task<int> CreateNotificationAsync(Notification notification);
        Task<IEnumerable<Notification>> GetNotificationsByUserAsync(int userId);
        Task MarkNotificationReadAsync(int notificationId);
    }

    public interface IFeedbackRepository
    {
        Task<int> CreateFeedbackAsync(Feedback feedback);
        Task<IEnumerable<Feedback>> GetAllFeedbackAsync();
    }

    public interface IAuditLogRepository
    {
        Task CreateAuditLogAsync(AuditLog log);
        Task<IEnumerable<AuditLog>> GetAuditLogsAsync();
    }

    public interface IDashboardRepository
    {
        Task<AdminDashboardStatsDto> GetAdminDashboardStatsAsync(int? branchId, int? year, int? month, DateTime? startDate, DateTime? endDate);
    }
}
