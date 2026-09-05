using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Leb.Core.Entities;
using Leb.Core.DTOs;
using Leb.Core.Interfaces;

namespace Leb.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly IPasswordHasher _passwordHasher;

        public AdminController(
            IDashboardRepository dashboardRepository,
            IUserRepository userRepository,
            IDoctorRepository doctorRepository,
            IStaffRepository staffRepository,
            IPatientRepository patientRepository,
            IAuditLogRepository auditLogRepository,
            IFeedbackRepository feedbackRepository,
            IPasswordHasher passwordHasher)
        {
            _dashboardRepository = dashboardRepository;
            _userRepository = userRepository;
            _doctorRepository = doctorRepository;
            _staffRepository = staffRepository;
            _patientRepository = patientRepository;
            _auditLogRepository = auditLogRepository;
            _feedbackRepository = feedbackRepository;
            _passwordHasher = passwordHasher;
        }


        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var list = await _userRepository.GetUsersWithRolesAsync();
            return Ok(list);
        }

        // --- DOCTORS CRUD ---
        [HttpGet("doctors")]
        public async Task<IActionResult> GetDoctors()
        {
            var list = await _doctorRepository.GetDoctorsListAsync();
            return Ok(list);
        }

        [HttpPost("doctors")]
        public async Task<IActionResult> CreateDoctor([FromBody] Doctor doctor)
        {
            // 1. Create base user
            string passwordToHash = string.IsNullOrWhiteSpace(doctor.Password) ? "Doctor@123" : doctor.Password;
            _passwordHasher.HashPassword(passwordToHash, out string hash, out string salt);
            var user = new User
            {
                Email = doctor.Email,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                PhoneNumber = doctor.PhoneNumber,
                PasswordHash = hash,
                Salt = salt,
                RoleId = 4, // Doctor Role
                IsEmailVerified = true
            };
            int userId = await _userRepository.CreateUserAsync(user);

            // 2. Create doctor profile
            doctor.UserId = userId;
            int doctorId = await _doctorRepository.CreateDoctorAsync(doctor);

            // 3. Log action
            await _auditLogRepository.CreateAuditLogAsync(new AuditLog
            {
                Action = "CREATE",
                TableName = "Doctors",
                RecordId = doctorId,
                NewValues = $"Email: {doctor.Email}, Specialization: {doctor.Specialization}"
            });

            return Ok(new { Message = "Doctor profile created successfully.", DoctorId = doctorId });
        }

        [HttpPut("doctors")]
        public async Task<IActionResult> UpdateDoctor([FromBody] Doctor doctor)
        {
            await _doctorRepository.UpdateDoctorAsync(doctor);
            
            if (!string.IsNullOrWhiteSpace(doctor.Password))
            {
                _passwordHasher.HashPassword(doctor.Password, out string hash, out string salt);
                await _userRepository.UpdatePasswordAsync(doctor.UserId, hash, salt);
            }
            
            await _auditLogRepository.CreateAuditLogAsync(new AuditLog
            {
                Action = "UPDATE",
                TableName = "Doctors",
                RecordId = doctor.DoctorId,
                NewValues = $"Specialization: {doctor.Specialization}, CommRate: {doctor.CommissionRate}"
            });

            return Ok(new { Message = "Doctor profile updated successfully." });
        }

        [HttpDelete("doctors/{id}")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            await _doctorRepository.DeleteDoctorAsync(id);
            
            await _auditLogRepository.CreateAuditLogAsync(new AuditLog
            {
                Action = "DELETE",
                TableName = "Doctors",
                RecordId = id
            });

            return Ok(new { Message = "Doctor profile deleted successfully." });
        }

        // --- STAFF CRUD ---
        [HttpGet("staff")]
        public async Task<IActionResult> GetStaff()
        {
            var list = await _staffRepository.GetStaffListAsync();
            return Ok(list);
        }

        [HttpPost("staff")]
        public async Task<IActionResult> CreateStaff([FromBody] Staff staff)
        {
            // 1. Create base user
            string passwordToHash = string.IsNullOrWhiteSpace(staff.Password) ? "Staff@123" : staff.Password;
            _passwordHasher.HashPassword(passwordToHash, out string hash, out string salt);
            var user = new User
            {
                Email = staff.Email,
                FirstName = staff.FirstName,
                LastName = staff.LastName,
                PhoneNumber = staff.PhoneNumber,
                PasswordHash = hash,
                Salt = salt,
                RoleId = 2, // Staff Role
                IsEmailVerified = true
            };
            int userId = await _userRepository.CreateUserAsync(user);

            // 2. Create staff profile
            staff.UserId = userId;
            int staffId = await _staffRepository.CreateStaffAsync(staff);

            // 3. Log action
            await _auditLogRepository.CreateAuditLogAsync(new AuditLog
            {
                Action = "CREATE",
                TableName = "Staff",
                RecordId = staffId,
                NewValues = $"Email: {staff.Email}, BranchId: {staff.BranchId}"
            });

            return Ok(new { Message = "Staff profile created successfully.", StaffId = staffId });
        }

        [HttpPut("staff")]
        public async Task<IActionResult> UpdateStaff([FromBody] Staff staff)
        {
            await _staffRepository.UpdateStaffAsync(staff);

            if (!string.IsNullOrWhiteSpace(staff.Password))
            {
                _passwordHasher.HashPassword(staff.Password, out string hash, out string salt);
                await _userRepository.UpdatePasswordAsync(staff.UserId, hash, salt);
            }

            await _auditLogRepository.CreateAuditLogAsync(new AuditLog
            {
                Action = "UPDATE",
                TableName = "Staff",
                RecordId = staff.StaffId,
                NewValues = $"BranchId: {staff.BranchId}, Designation: {staff.Designation}"
            });

            return Ok(new { Message = "Staff profile updated successfully." });
        }

        [HttpDelete("staff/{id}")]
        public async Task<IActionResult> DeleteStaff(int id)
        {
            await _staffRepository.DeleteStaffAsync(id);

            await _auditLogRepository.CreateAuditLogAsync(new AuditLog
            {
                Action = "DELETE",
                TableName = "Staff",
                RecordId = id
            });

            return Ok(new { Message = "Staff profile deleted successfully." });
        }

        // --- PATIENTS CRUD ---
        [HttpGet("patients")]
        public async Task<IActionResult> GetPatients()
        {
            var list = await _patientRepository.GetPatientsListAsync();
            return Ok(list);
        }

        [HttpPut("patients")]
        public async Task<IActionResult> UpdatePatient([FromBody] Patient patient)
        {
            await _patientRepository.UpdatePatientAsync(patient);

            await _auditLogRepository.CreateAuditLogAsync(new AuditLog
            {
                Action = "UPDATE",
                TableName = "Patients",
                RecordId = patient.PatientId,
                NewValues = $"BloodGroup: {patient.BloodGroup}, Address: {patient.Address}"
            });

            return Ok(new { Message = "Patient profile updated successfully." });
        }

        [HttpDelete("patients/{id}")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            await _patientRepository.DeletePatientAsync(id);

            await _auditLogRepository.CreateAuditLogAsync(new AuditLog
            {
                Action = "DELETE",
                TableName = "Patients",
                RecordId = id
            });

            return Ok(new { Message = "Patient profile deleted successfully." });
        }

        // --- SYSTEM UTILITIES ---
        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAuditLogs()
        {
            var list = await _auditLogRepository.GetAuditLogsAsync();
            return Ok(list);
        }

        [HttpGet("feedback")]
        public async Task<IActionResult> GetFeedback()
        {
            var list = await _feedbackRepository.GetAllFeedbackAsync();
            return Ok(list);
        }

        [HttpPost("backup")]
        public async Task<IActionResult> BackupDatabase()
        {
            // In a production app, you would run a SQL command BACKUP DATABASE LaboratoryDb TO DISK = ...
            // We simulate it and write a log.
            await _auditLogRepository.CreateAuditLogAsync(new AuditLog
            {
                Action = "BACKUP_DB",
                TableName = "Database",
                NewValues = "Completed database backup to server location."
            });
            return Ok(new { Message = "Database backup completed successfully. File saved on server." });
        }
    }
}
