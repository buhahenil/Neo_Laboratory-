using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Leb.Core.Entities;
using Leb.Core.DTOs;
using Leb.Core.Interfaces;

namespace Leb.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJWTService _jwtService;
        private readonly IEmailService _emailService;

        public AuthController(
            IUserRepository userRepository,
            IPatientRepository patientRepository,
            IStaffRepository staffRepository,
            IDoctorRepository doctorRepository,
            IBranchRepository branchRepository,
            IPasswordHasher passwordHasher,
            IJWTService jwtService,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _patientRepository = patientRepository;
            _staffRepository = staffRepository;
            _doctorRepository = doctorRepository;
            _branchRepository = branchRepository;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { Message = "Email is already registered." });
            }

            _passwordHasher.HashPassword(dto.Password, out string hash, out string salt);

            var verificationToken = Guid.NewGuid().ToString("N");

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = hash,
                Salt = salt,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                RoleId = 3, // Default to Patient
                IsEmailVerified = false,
                VerificationToken = verificationToken
            };

            int userId = await _userRepository.CreateUserAsync(user);

            var patient = new Patient
            {
                UserId = userId,
                DateOfBirth = dto.DateOfBirth ?? DateTime.Today.AddYears(-20),
                Gender = dto.Gender ?? "Male",
                BloodGroup = dto.BloodGroup,
                Address = dto.Address
            };

            await _patientRepository.CreatePatientAsync(patient);

            // Send Mock Verification Email
            string verificationUrl = $"{Request.Scheme}://{Request.Host}/api/auth/verify-email?token={verificationToken}";
            await _emailService.SendEmailAsync(dto.Email, "Verify Your Email - Diagnostic Lab", 
                $"Hello {dto.FirstName},\n\nPlease verify your email by clicking: {verificationUrl}");

            return Ok(new { Message = "Registration successful. Please check your email to verify." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userRepository.GetUserByEmailAsync(dto.Email);
            if (user == null || !_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash, user.Salt))
            {
                return Unauthorized(new { Message = "Invalid email or password." });
            }

            int? patientId = null;
            int? staffId = null;
            int? doctorId = null;
            int? branchId = null;
            string? branchName = null;
            string? designation = null;

            // Fetch contextual IDs based on role
            if (user.RoleName == "Patient")
            {
                var patient = await _patientRepository.GetPatientByUserIdAsync(user.UserId);
                patientId = patient?.PatientId;
            }
            else if (user.RoleName == "Staff")
            {
                var staffList = await _staffRepository.GetStaffListAsync();
                Staff? staff = null;
                foreach (var s in staffList)
                {
                    if (s.UserId == user.UserId) staff = s;
                }

                if (staff == null)
                {
                    return BadRequest(new { Message = "Staff profile not found." });
                }

                if (!dto.BranchId.HasValue || dto.BranchId.Value == 0)
                {
                    return BadRequest(new { Message = "Please select the branch you wish to log into." });
                }

                if (staff.BranchId != dto.BranchId.Value && !staff.BranchIds.Contains(dto.BranchId.Value))
                {
                    return StatusCode(403, new { Message = "You are not authorized to access this branch." });
                }

                staffId = staff.StaffId;
                branchId = dto.BranchId.Value;
                designation = staff.Designation;

                var branch = await _branchRepository.GetBranchByIdAsync(branchId.Value);
                branchName = branch?.Name;
            }
            else if (user.RoleName == "Doctor")
            {
                var doctorList = await _doctorRepository.GetDoctorsListAsync();
                foreach (var d in doctorList)
                {
                    if (d.UserId == user.UserId)
                    {
                        doctorId = d.DoctorId;
                        designation = d.Designation;
                    }
                }
            }

            var token = _jwtService.GenerateToken(user, patientId, staffId, doctorId, branchId);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.RoleName,
                UserId = user.UserId,
                PatientId = patientId,
                StaffId = staffId,
                DoctorId = doctorId,
                BranchId = branchId,
                BranchName = branchName,
                Designation = designation
            });
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            bool verified = await _userRepository.VerifyEmailAsync(token);
            if (!verified)
            {
                return BadRequest(new { Message = "Invalid or expired verification token." });
            }
            return Ok(new { Message = "Email verified successfully! You can now log in." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var token = Guid.NewGuid().ToString("N");
            var expiry = DateTime.UtcNow.AddHours(2);
            
            bool set = await _userRepository.SetResetTokenAsync(dto.Email, token, expiry);
            if (!set)
            {
                return BadRequest(new { Message = "Email address not found." });
            }

            await _emailService.SendEmailAsync(dto.Email, "Reset Your Password - Diagnostic Lab", 
                $"To reset your password, please use the following token: {token}");

            return Ok(new { Message = "Password reset instructions sent to your email." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var user = await _userRepository.GetUserByResetTokenAsync(dto.Token);
            if (user == null)
            {
                return BadRequest(new { Message = "Invalid or expired reset token." });
            }

            _passwordHasher.HashPassword(dto.NewPassword, out string hash, out string salt);
            await _userRepository.UpdatePasswordAsync(user.UserId, hash, salt);

            return Ok(new { Message = "Password reset successfully. You can now log in." });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var user = await _userRepository.GetUserByIdAsync(dto.UserId);
            if (user == null || !_passwordHasher.VerifyPassword(dto.OldPassword, user.PasswordHash, user.Salt))
            {
                return BadRequest(new { Message = "Incorrect old password." });
            }

            _passwordHasher.HashPassword(dto.NewPassword, out string hash, out string salt);
            await _userRepository.UpdatePasswordAsync(user.UserId, hash, salt);

            return Ok(new { Message = "Password changed successfully." });
        }

        [Authorize]
        [HttpGet("staff-profile/{userId}")]
        public async Task<IActionResult> GetStaffProfile(int userId)
        {
            var staffList = await _staffRepository.GetStaffListAsync();
            foreach (var s in staffList)
            {
                if (s.UserId == userId) return Ok(s);
            }
            return NotFound(new { Message = "Staff profile not found." });
        }
    }
}
