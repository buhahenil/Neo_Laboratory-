using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Leb.Core.Entities;
using Leb.Core.DTOs;
using Leb.Core.Interfaces;
using Leb.Infrastructure.Data;

namespace Leb.Infrastructure.Repositories
{
    // --- USER REPOSITORY ---
    public class UserRepository : IUserRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;
        public UserRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<int> CreateUserAsync(User user)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_CreateUser", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Email", user.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            cmd.Parameters.AddWithValue("@Salt", user.Salt);
            cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
            cmd.Parameters.AddWithValue("@LastName", user.LastName);
            cmd.Parameters.AddWithValue("@PhoneNumber", (object?)user.PhoneNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RoleId", user.RoleId);
            cmd.Parameters.AddWithValue("@IsEmailVerified", user.IsEmailVerified);
            cmd.Parameters.AddWithValue("@VerificationToken", (object?)user.VerificationToken ?? DBNull.Value);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetUserById", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@UserId", userId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapUser(reader);
            }
            return null;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetUserByEmail", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Email", email);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapUser(reader);
            }
            return null;
        }

        public async Task UpdateUserAsync(User user)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_UpdateUser", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@UserId", user.UserId);
            cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
            cmd.Parameters.AddWithValue("@LastName", user.LastName);
            cmd.Parameters.AddWithValue("@PhoneNumber", (object?)user.PhoneNumber ?? DBNull.Value);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteUserAsync(int userId)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_DeleteUser", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@UserId", userId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdatePasswordAsync(int userId, string passwordHash, string salt)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_UpdatePassword", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
            cmd.Parameters.AddWithValue("@Salt", salt);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_VerifyEmail", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@VerificationToken", token);

            await conn.OpenAsync();
            var rows = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(rows) > 0;
        }

        public async Task<bool> SetResetTokenAsync(string email, string token, DateTime expiry)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_SetResetToken", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@ResetToken", token);
            cmd.Parameters.AddWithValue("@Expiry", expiry);

            await conn.OpenAsync();
            var rows = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(rows) > 0;
        }

        public async Task<User?> GetUserByResetTokenAsync(string token)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetResetToken", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@ResetToken", token);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapUser(reader);
            }
            return null;
        }

        public async Task<IEnumerable<User>> GetUsersWithRolesAsync()
        {
            var list = new List<User>();
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetUsersWithRoles", conn) { CommandType = CommandType.StoredProcedure };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new User
                {
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                    PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                    RoleId = reader.GetInt32(reader.GetOrdinal("RoleId")),
                    RoleName = reader.GetString(reader.GetOrdinal("RoleName")),
                    IsEmailVerified = reader.GetBoolean(reader.GetOrdinal("IsEmailVerified")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                });
            }
            return list;
        }

        private static User MapUser(SqlDataReader reader)
        {
            return new User
            {
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                Salt = reader.GetString(reader.GetOrdinal("Salt")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                RoleId = reader.GetInt32(reader.GetOrdinal("RoleId")),
                RoleName = reader.GetString(reader.GetOrdinal("RoleName")),
                IsEmailVerified = reader.GetBoolean(reader.GetOrdinal("IsEmailVerified")),
                VerificationToken = reader.IsDBNull(reader.GetOrdinal("VerificationToken")) ? null : reader.GetString(reader.GetOrdinal("VerificationToken")),
                ResetToken = reader.IsDBNull(reader.GetOrdinal("ResetToken")) ? null : reader.GetString(reader.GetOrdinal("ResetToken")),
                ResetTokenExpiry = reader.IsDBNull(reader.GetOrdinal("ResetTokenExpiry")) ? null : reader.GetDateTime(reader.GetOrdinal("ResetTokenExpiry")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
    }

    // --- PATIENT REPOSITORY ---
    public class PatientRepository : IPatientRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;
        public PatientRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<int> CreatePatientAsync(Patient patient)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_CreatePatient", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@UserId", patient.UserId);
            cmd.Parameters.AddWithValue("@DateOfBirth", patient.DateOfBirth);
            cmd.Parameters.AddWithValue("@Gender", patient.Gender);
            cmd.Parameters.AddWithValue("@BloodGroup", (object?)patient.BloodGroup ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object?)patient.Address ?? DBNull.Value);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<Patient?> GetPatientByIdAsync(int patientId)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetPatientById", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@PatientId", patientId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapPatient(reader);
            }
            return null;
        }

        public async Task<Patient?> GetPatientByUserIdAsync(int userId)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetPatientByUserId", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@UserId", userId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapPatient(reader);
            }
            return null;
        }

        public async Task UpdatePatientAsync(Patient patient)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_UpdatePatient", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@PatientId", patient.PatientId);
            cmd.Parameters.AddWithValue("@DateOfBirth", patient.DateOfBirth);
            cmd.Parameters.AddWithValue("@Gender", patient.Gender);
            cmd.Parameters.AddWithValue("@BloodGroup", (object?)patient.BloodGroup ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object?)patient.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FirstName", patient.FirstName);
            cmd.Parameters.AddWithValue("@LastName", patient.LastName);
            cmd.Parameters.AddWithValue("@PhoneNumber", (object?)patient.PhoneNumber ?? DBNull.Value);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeletePatientAsync(int patientId)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_DeletePatient", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@PatientId", patientId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<Patient>> GetPatientsListAsync()
        {
            var list = new List<Patient>();
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetPatientsList", conn) { CommandType = CommandType.StoredProcedure };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapPatient(reader));
            }
            return list;
        }

        private static Patient MapPatient(SqlDataReader reader)
        {
            return new Patient
            {
                PatientId = reader.GetInt32(reader.GetOrdinal("PatientId")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                DateOfBirth = reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
                Gender = reader.GetString(reader.GetOrdinal("Gender")),
                BloodGroup = reader.IsDBNull(reader.GetOrdinal("BloodGroup")) ? null : reader.GetString(reader.GetOrdinal("BloodGroup")),
                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber"))
            };
        }
    }

    // --- DOCTOR REPOSITORY ---
    public class DoctorRepository : IDoctorRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;
        public DoctorRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<int> CreateDoctorAsync(Doctor doctor)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_CreateDoctor", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@UserId", doctor.UserId);
            cmd.Parameters.AddWithValue("@Specialization", doctor.Specialization);
            cmd.Parameters.AddWithValue("@Designation", doctor.Designation);
            cmd.Parameters.AddWithValue("@CommissionRate", doctor.CommissionRate);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<Doctor?> GetDoctorByIdAsync(int doctorId)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetDoctorById", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@DoctorId", doctorId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapDoctor(reader);
            }
            return null;
        }

        public async Task UpdateDoctorAsync(Doctor doctor)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_UpdateDoctor", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@DoctorId", doctor.DoctorId);
            cmd.Parameters.AddWithValue("@Specialization", doctor.Specialization);
            cmd.Parameters.AddWithValue("@Designation", doctor.Designation);
            cmd.Parameters.AddWithValue("@CommissionRate", doctor.CommissionRate);
            cmd.Parameters.AddWithValue("@FirstName", doctor.FirstName);
            cmd.Parameters.AddWithValue("@LastName", doctor.LastName);
            cmd.Parameters.AddWithValue("@PhoneNumber", (object?)doctor.PhoneNumber ?? DBNull.Value);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteDoctorAsync(int doctorId)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_DeleteDoctor", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@DoctorId", doctorId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<Doctor>> GetDoctorsListAsync()
        {
            var list = new List<Doctor>();
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetDoctorsList", conn) { CommandType = CommandType.StoredProcedure };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapDoctor(reader));
            }
            return list;
        }

        private static Doctor MapDoctor(SqlDataReader reader)
        {
            return new Doctor
            {
                DoctorId = reader.GetInt32(reader.GetOrdinal("DoctorId")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                Specialization = reader.GetString(reader.GetOrdinal("Specialization")),
                Designation = reader.GetString(reader.GetOrdinal("Designation")),
                CommissionRate = reader.GetDecimal(reader.GetOrdinal("CommissionRate")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber"))
            };
        }
    }

    // --- STAFF REPOSITORY ---
    public class StaffRepository : IStaffRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;
        public StaffRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<int> CreateStaffAsync(Staff staff)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_CreateStaff", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@UserId", staff.UserId);
            cmd.Parameters.AddWithValue("@BranchId", staff.BranchId);
            cmd.Parameters.AddWithValue("@Designation", staff.Designation);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            int staffId = Convert.ToInt32(result);

            if (staff.BranchIds != null && staff.BranchIds.Count > 0)
            {
                foreach (var branchId in staff.BranchIds)
                {
                    using var bCmd = new SqlCommand("INSERT INTO dbo.StaffBranches (StaffId, BranchId) VALUES (@StaffId, @BranchId)", conn);
                    bCmd.Parameters.AddWithValue("@StaffId", staffId);
                    bCmd.Parameters.AddWithValue("@BranchId", branchId);
                    await bCmd.ExecuteNonQueryAsync();
                }
            }
            else
            {
                using var bCmd = new SqlCommand("INSERT INTO dbo.StaffBranches (StaffId, BranchId) VALUES (@StaffId, @BranchId)", conn);
                bCmd.Parameters.AddWithValue("@StaffId", staffId);
                bCmd.Parameters.AddWithValue("@BranchId", staff.BranchId);
                await bCmd.ExecuteNonQueryAsync();
            }

            return staffId;
        }

        public async Task<Staff?> GetStaffByIdAsync(int staffId)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetStaffById", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@StaffId", staffId);

            await conn.OpenAsync();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    var staff = MapStaff(reader);
                    reader.Close();
                    using var bCmd = new SqlCommand("SELECT BranchId FROM dbo.StaffBranches WHERE StaffId = @StaffId", conn);
                    bCmd.Parameters.AddWithValue("@StaffId", staffId);
                    using var bReader = await bCmd.ExecuteReaderAsync();
                    while (await bReader.ReadAsync())
                    {
                        staff.BranchIds.Add(bReader.GetInt32(0));
                    }
                    return staff;
                }
            }
            return null;
        }

        public async Task UpdateStaffAsync(Staff staff)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_UpdateStaff", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@StaffId", staff.StaffId);
            cmd.Parameters.AddWithValue("@BranchId", staff.BranchId);
            cmd.Parameters.AddWithValue("@Designation", staff.Designation);
            cmd.Parameters.AddWithValue("@FirstName", staff.FirstName);
            cmd.Parameters.AddWithValue("@LastName", staff.LastName);
            cmd.Parameters.AddWithValue("@PhoneNumber", (object?)staff.PhoneNumber ?? DBNull.Value);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            using var deleteCmd = new SqlCommand("DELETE FROM dbo.StaffBranches WHERE StaffId = @StaffId", conn);
            deleteCmd.Parameters.AddWithValue("@StaffId", staff.StaffId);
            await deleteCmd.ExecuteNonQueryAsync();

            if (staff.BranchIds != null && staff.BranchIds.Count > 0)
            {
                foreach (var branchId in staff.BranchIds)
                {
                    using var bCmd = new SqlCommand("INSERT INTO dbo.StaffBranches (StaffId, BranchId) VALUES (@StaffId, @BranchId)", conn);
                    bCmd.Parameters.AddWithValue("@StaffId", staff.StaffId);
                    bCmd.Parameters.AddWithValue("@BranchId", branchId);
                    await bCmd.ExecuteNonQueryAsync();
                }
            }
            else
            {
                using var bCmd = new SqlCommand("INSERT INTO dbo.StaffBranches (StaffId, BranchId) VALUES (@StaffId, @BranchId)", conn);
                bCmd.Parameters.AddWithValue("@StaffId", staff.StaffId);
                bCmd.Parameters.AddWithValue("@BranchId", staff.BranchId);
                await bCmd.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteStaffAsync(int staffId)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_DeleteStaff", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@StaffId", staffId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<Staff>> GetStaffListAsync()
        {
            var list = new List<Staff>();
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetStaffList", conn) { CommandType = CommandType.StoredProcedure };

            await conn.OpenAsync();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    list.Add(new Staff
                    {
                        StaffId = reader.GetInt32(reader.GetOrdinal("StaffId")),
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        BranchId = reader.GetInt32(reader.GetOrdinal("BranchId")),
                        BranchName = reader.GetString(reader.GetOrdinal("BranchName")),
                        Designation = reader.GetString(reader.GetOrdinal("Designation")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                        LastName = reader.GetString(reader.GetOrdinal("LastName")),
                        PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber"))
                    });
                }
            }

            foreach (var staff in list)
            {
                using var bCmd = new SqlCommand("SELECT BranchId FROM dbo.StaffBranches WHERE StaffId = @StaffId", conn);
                bCmd.Parameters.AddWithValue("@StaffId", staff.StaffId);
                using var bReader = await bCmd.ExecuteReaderAsync();
                while (await bReader.ReadAsync())
                {
                    staff.BranchIds.Add(bReader.GetInt32(0));
                }
            }

            return list;
        }

        private static Staff MapStaff(SqlDataReader reader)
        {
            return new Staff
            {
                StaffId = reader.GetInt32(reader.GetOrdinal("StaffId")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                BranchId = reader.GetInt32(reader.GetOrdinal("BranchId")),
                BranchName = reader.GetString(reader.GetOrdinal("BranchName")),
                Designation = reader.GetString(reader.GetOrdinal("Designation")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber"))
            };
        }
    }

    // --- BRANCH REPOSITORY ---
    public class BranchRepository : IBranchRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;
        public BranchRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<int> CreateBranchAsync(Branch branch)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_CreateBranch", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Name", branch.Name);
            cmd.Parameters.AddWithValue("@Address", branch.Address);
            cmd.Parameters.AddWithValue("@City", branch.City);
            cmd.Parameters.AddWithValue("@ContactNumber", branch.ContactNumber);
            cmd.Parameters.AddWithValue("@Email", branch.Email);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<Branch?> GetBranchByIdAsync(int branchId)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetBranchById", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@BranchId", branchId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapBranch(reader);
            }
            return null;
        }

        public async Task UpdateBranchAsync(Branch branch)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_UpdateBranch", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@BranchId", branch.BranchId);
            cmd.Parameters.AddWithValue("@Name", branch.Name);
            cmd.Parameters.AddWithValue("@Address", branch.Address);
            cmd.Parameters.AddWithValue("@City", branch.City);
            cmd.Parameters.AddWithValue("@ContactNumber", branch.ContactNumber);
            cmd.Parameters.AddWithValue("@Email", branch.Email);
            cmd.Parameters.AddWithValue("@IsActive", branch.IsActive);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteBranchAsync(int branchId)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_DeleteBranch", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@BranchId", branchId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<Branch>> GetAllBranchesAsync()
        {
            var list = new List<Branch>();
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetAllBranches", conn) { CommandType = CommandType.StoredProcedure };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapBranch(reader));
            }
            return list;
        }

        private static Branch MapBranch(SqlDataReader reader)
        {
            return new Branch
            {
                BranchId = reader.GetInt32(reader.GetOrdinal("BranchId")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Address = reader.GetString(reader.GetOrdinal("Address")),
                City = reader.GetString(reader.GetOrdinal("City")),
                ContactNumber = reader.GetString(reader.GetOrdinal("ContactNumber")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
    }

    // --- TIME SLOT REPOSITORY ---
    public class TimeSlotRepository : ITimeSlotRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;
        public TimeSlotRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<int> CreateTimeSlotAsync(TimeSlot slot)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_CreateTimeSlot", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@BranchId", slot.BranchId);
            cmd.Parameters.AddWithValue("@StartTime", slot.StartTime);
            cmd.Parameters.AddWithValue("@EndTime", slot.EndTime);
            cmd.Parameters.AddWithValue("@MaxBookings", slot.MaxBookings);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<IEnumerable<TimeSlot>> GetSlotsByBranchAsync(int branchId)
        {
            var list = new List<TimeSlot>();
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetSlotsByBranch", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@BranchId", branchId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new TimeSlot
                {
                    SlotId = reader.GetInt32(reader.GetOrdinal("SlotId")),
                    BranchId = reader.GetInt32(reader.GetOrdinal("BranchId")),
                    StartTime = reader.GetTimeSpan(reader.GetOrdinal("StartTime")),
                    EndTime = reader.GetTimeSpan(reader.GetOrdinal("EndTime")),
                    MaxBookings = reader.GetInt32(reader.GetOrdinal("MaxBookings")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                });
            }
            return list;
        }

        public async Task UpdateTimeSlotAsync(TimeSlot slot)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_UpdateTimeSlot", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@SlotId", slot.SlotId);
            cmd.Parameters.AddWithValue("@StartTime", slot.StartTime);
            cmd.Parameters.AddWithValue("@EndTime", slot.EndTime);
            cmd.Parameters.AddWithValue("@MaxBookings", slot.MaxBookings);
            cmd.Parameters.AddWithValue("@IsActive", slot.IsActive);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteTimeSlotAsync(int slotId)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_DeleteTimeSlot", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@SlotId", slotId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }

    // --- HOLIDAY REPOSITORY ---
    public class HolidayRepository : IHolidayRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;
        public HolidayRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<int> CreateHolidayAsync(Holiday holiday)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_CreateHoliday", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@BranchId", holiday.BranchId);
            cmd.Parameters.AddWithValue("@HolidayDate", holiday.HolidayDate);
            cmd.Parameters.AddWithValue("@Description", (object?)holiday.Description ?? DBNull.Value);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<IEnumerable<Holiday>> GetHolidaysByBranchAsync(int branchId)
        {
            var list = new List<Holiday>();
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetHolidaysByBranch", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@BranchId", branchId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Holiday
                {
                    HolidayId = reader.GetInt32(reader.GetOrdinal("HolidayId")),
                    BranchId = reader.GetInt32(reader.GetOrdinal("BranchId")),
                    HolidayDate = reader.GetDateTime(reader.GetOrdinal("HolidayDate")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description"))
                });
            }
            return list;
        }

        public async Task DeleteHolidayAsync(int holidayId)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_DeleteHoliday", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@HolidayId", holidayId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }

    // --- TEST CATEGORY REPOSITORY ---
    public class TestCategoryRepository : ITestCategoryRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;
        public TestCategoryRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<int> CreateCategoryAsync(TestCategory category)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_CreateTestCategory", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Name", category.Name);
            cmd.Parameters.AddWithValue("@Description", (object?)category.Description ?? DBNull.Value);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<TestCategory?> GetCategoryByIdAsync(int categoryId)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetTestCategoryById", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@CategoryId", categoryId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new TestCategory
                {
                    CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description"))
                };
            }
            return null;
        }

        public async Task UpdateCategoryAsync(TestCategory category)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_UpdateTestCategory", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@CategoryId", category.CategoryId);
            cmd.Parameters.AddWithValue("@Name", category.Name);
            cmd.Parameters.AddWithValue("@Description", (object?)category.Description ?? DBNull.Value);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteCategoryAsync(int categoryId)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_DeleteTestCategory", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@CategoryId", categoryId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<TestCategory>> GetAllCategoriesAsync()
        {
            var list = new List<TestCategory>();
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetAllTestCategories", conn) { CommandType = CommandType.StoredProcedure };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new TestCategory
                {
                    CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description"))
                });
            }
            return list;
        }
    }

    // --- TEST REPOSITORY ---
    public class TestRepository : ITestRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;
        public TestRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<int> CreateTestAsync(Test test)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_CreateTest", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@CategoryId", test.CategoryId);
            cmd.Parameters.AddWithValue("@Name", test.Name);
            cmd.Parameters.AddWithValue("@Code", test.Code);
            cmd.Parameters.AddWithValue("@Description", (object?)test.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Price", test.Price);
            cmd.Parameters.AddWithValue("@SampleType", test.SampleType);
            cmd.Parameters.AddWithValue("@Preparation", (object?)test.Preparation ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NormalRange", (object?)test.NormalRange ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DeliveryTimeHours", test.DeliveryTimeHours);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<Test?> GetTestByIdAsync(int testId)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetTestById", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@TestId", testId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapTest(reader);
            }
            return null;
        }

        public async Task UpdateTestAsync(Test test)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_UpdateTest", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@TestId", test.TestId);
            cmd.Parameters.AddWithValue("@CategoryId", test.CategoryId);
            cmd.Parameters.AddWithValue("@Name", test.Name);
            cmd.Parameters.AddWithValue("@Code", test.Code);
            cmd.Parameters.AddWithValue("@Description", (object?)test.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Price", test.Price);
            cmd.Parameters.AddWithValue("@SampleType", test.SampleType);
            cmd.Parameters.AddWithValue("@Preparation", (object?)test.Preparation ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NormalRange", (object?)test.NormalRange ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DeliveryTimeHours", test.DeliveryTimeHours);
            cmd.Parameters.AddWithValue("@IsActive", test.IsActive);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteTestAsync(int testId)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_DeleteTest", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@TestId", testId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<Test>> GetAllTestsAsync()
        {
            var list = new List<Test>();
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetAllTests", conn) { CommandType = CommandType.StoredProcedure };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapTest(reader));
            }
            return list;
        }

        private static Test MapTest(SqlDataReader reader)
        {
            return new Test
            {
                TestId = reader.GetInt32(reader.GetOrdinal("TestId")),
                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Code = reader.GetString(reader.GetOrdinal("Code")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                SampleType = reader.GetString(reader.GetOrdinal("SampleType")),
                Preparation = reader.IsDBNull(reader.GetOrdinal("Preparation")) ? null : reader.GetString(reader.GetOrdinal("Preparation")),
                NormalRange = reader.IsDBNull(reader.GetOrdinal("NormalRange")) ? null : reader.GetString(reader.GetOrdinal("NormalRange")),
                DeliveryTimeHours = reader.GetInt32(reader.GetOrdinal("DeliveryTimeHours")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            };
        }
    }

    // --- PACKAGE REPOSITORY ---
    public class PackageRepository : IPackageRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;
        public PackageRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<int> CreatePackageAsync(Package package)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_CreatePackage", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Name", package.Name);
            cmd.Parameters.AddWithValue("@Code", package.Code);
            cmd.Parameters.AddWithValue("@Description", (object?)package.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Price", package.Price);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<Package?> GetPackageByIdAsync(int packageId)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetPackageById", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@PackageId", packageId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Package
                {
                    PackageId = reader.GetInt32(reader.GetOrdinal("PackageId")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Code = reader.GetString(reader.GetOrdinal("Code")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                };
            }
            return null;
        }

        public async Task UpdatePackageAsync(Package package)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_UpdatePackage", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@PackageId", package.PackageId);
            cmd.Parameters.AddWithValue("@Name", package.Name);
            cmd.Parameters.AddWithValue("@Code", package.Code);
            cmd.Parameters.AddWithValue("@Description", (object?)package.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Price", package.Price);
            cmd.Parameters.AddWithValue("@IsActive", package.IsActive);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeletePackageAsync(int packageId)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_DeletePackage", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@PackageId", packageId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<Package>> GetAllPackagesAsync()
        {
            var list = new List<Package>();
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetAllPackages", conn) { CommandType = CommandType.StoredProcedure };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Package
                {
                    PackageId = reader.GetInt32(reader.GetOrdinal("PackageId")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Code = reader.GetString(reader.GetOrdinal("Code")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                });
            }
            return list;
        }

        public async Task AddTestToPackageAsync(int packageId, int testId)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_AddTestToPackage", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@PackageId", packageId);
            cmd.Parameters.AddWithValue("@TestId", testId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task SystemRemoveTestFromPackage(int packageId, int testId) // Matching interface
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_RemoveTestFromPackage", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@PackageId", packageId);
            cmd.Parameters.AddWithValue("@TestId", testId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RemoveTestFromPackageAsync(int packageId, int testId)
        {
            await SystemRemoveTestFromPackage(packageId, testId);
        }

        public async Task<IEnumerable<Test>> GetPackageTestsAsync(int packageId)
        {
            var list = new List<Test>();
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetPackageTests", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@PackageId", packageId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Test
                {
                    TestId = reader.GetInt32(reader.GetOrdinal("TestId")),
                    CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                    CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Code = reader.GetString(reader.GetOrdinal("Code")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                    SampleType = reader.GetString(reader.GetOrdinal("SampleType")),
                    Preparation = reader.IsDBNull(reader.GetOrdinal("Preparation")) ? null : reader.GetString(reader.GetOrdinal("Preparation")),
                    NormalRange = reader.IsDBNull(reader.GetOrdinal("NormalRange")) ? null : reader.GetString(reader.GetOrdinal("NormalRange")),
                    DeliveryTimeHours = reader.GetInt32(reader.GetOrdinal("DeliveryTimeHours")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                });
            }
            return list;
        }
    }

    // --- APPOINTMENT REPOSITORY ---
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;
        public AppointmentRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<int> CreateAppointmentAsync(Appointment appointment, List<int> testIds, List<int> packageIds)
        {
            using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                using var cmd = new SqlCommand("dbo.sp_CreateAppointment", conn, transaction) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@PatientId", appointment.PatientId);
                cmd.Parameters.AddWithValue("@BranchId", appointment.BranchId);
                cmd.Parameters.AddWithValue("@AppointmentDate", appointment.AppointmentDate);
                cmd.Parameters.AddWithValue("@SlotId", appointment.SlotId);
                cmd.Parameters.AddWithValue("@TotalAmount", appointment.TotalAmount);
                cmd.Parameters.AddWithValue("@DiscountAmount", appointment.DiscountAmount);
                cmd.Parameters.AddWithValue("@PaidAmount", appointment.PaidAmount);
                cmd.Parameters.AddWithValue("@PaymentStatus", appointment.PaymentStatus);
                cmd.Parameters.AddWithValue("@Notes", (object?)appointment.Notes ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DoctorId", (object?)appointment.DoctorId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@StaffId", (object?)appointment.StaffId ?? DBNull.Value);

                var result = await cmd.ExecuteScalarAsync();
                int appointmentId = Convert.ToInt32(result);

                // Add individual tests
                foreach (var testId in testIds)
                {
                    // Find test price (we can fetch it or pass it. We'll read from DB or assume it's part of the items. 
                    // To be simple and direct: we query test price inside SP or pass a dummy, but let's query it or write SP logic)
                    // Wait, let's use sp_AddAppointmentTest. It expects AppointmentId, TestId, and Price. We will retrieve the test price first.
                    decimal price = 0;
                    using (var priceCmd = new SqlCommand("SELECT Price FROM dbo.Tests WHERE TestId = @TestId", conn, transaction))
                    {
                        priceCmd.Parameters.AddWithValue("@TestId", testId);
                        var priceObj = await priceCmd.ExecuteScalarAsync();
                        price = priceObj != null ? Convert.ToDecimal(priceObj) : 0;
                    }

                    using var testCmd = new SqlCommand("dbo.sp_AddAppointmentTest", conn, transaction) { CommandType = CommandType.StoredProcedure };
                    testCmd.Parameters.AddWithValue("@AppointmentId", appointmentId);
                    testCmd.Parameters.AddWithValue("@TestId", testId);
                    testCmd.Parameters.AddWithValue("@Price", price);
                    await testCmd.ExecuteNonQueryAsync();
                }

                // Add packages
                foreach (var pkgId in packageIds)
                {
                    decimal price = 0;
                    using (var priceCmd = new SqlCommand("SELECT Price FROM dbo.Packages WHERE PackageId = @PkgId", conn, transaction))
                    {
                        priceCmd.Parameters.AddWithValue("@PkgId", pkgId);
                        var priceObj = await priceCmd.ExecuteScalarAsync();
                        price = priceObj != null ? Convert.ToDecimal(priceObj) : 0;
                    }

                    using var pkgCmd = new SqlCommand("dbo.sp_AddAppointmentPackage", conn, transaction) { CommandType = CommandType.StoredProcedure };
                    pkgCmd.Parameters.AddWithValue("@AppointmentId", appointmentId);
                    pkgCmd.Parameters.AddWithValue("@PackageId", pkgId);
                    pkgCmd.Parameters.AddWithValue("@Price", price);
                    await pkgCmd.ExecuteNonQueryAsync();
                }

                transaction.Commit();
                return appointmentId;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(int appointmentId)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetAppointmentById", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                var appointment = MapAppointment(reader);

                // Read Tests (Second Result Set)
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        appointment.Tests.Add(new Test
                        {
                            TestId = reader.GetInt32(reader.GetOrdinal("TestId")),
                            CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                            CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Code = reader.GetString(reader.GetOrdinal("Code")),
                            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                            Price = reader.GetDecimal(reader.GetOrdinal("OrderedPrice")), // Use the price at booking
                            SampleType = reader.GetString(reader.GetOrdinal("SampleType")),
                            NormalRange = reader.IsDBNull(reader.GetOrdinal("NormalRange")) ? null : reader.GetString(reader.GetOrdinal("NormalRange")),
                            DeliveryTimeHours = reader.GetInt32(reader.GetOrdinal("DeliveryTimeHours"))
                        });
                    }
                }

                // Read Packages (Third Result Set)
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        appointment.Packages.Add(new Package
                        {
                            PackageId = reader.GetInt32(reader.GetOrdinal("PackageId")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Code = reader.GetString(reader.GetOrdinal("Code")),
                            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                            Price = reader.GetDecimal(reader.GetOrdinal("OrderedPrice"))
                        });
                    }
                }

                return appointment;
            }
            return null;
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByPatientAsync(int patientId)
        {
            var list = new List<Appointment>();
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetAppointmentsByPatient", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@PatientId", patientId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Appointment
                {
                    AppointmentId = reader.GetInt32(reader.GetOrdinal("AppointmentId")),
                    PatientId = reader.GetInt32(reader.GetOrdinal("PatientId")),
                    BranchId = reader.GetInt32(reader.GetOrdinal("BranchId")),
                    AppointmentDate = reader.GetDateTime(reader.GetOrdinal("AppointmentDate")),
                    SlotId = reader.GetInt32(reader.GetOrdinal("SlotId")),
                    Status = reader.GetString(reader.GetOrdinal("Status")),
                    TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                    DiscountAmount = reader.GetDecimal(reader.GetOrdinal("DiscountAmount")),
                    PaidAmount = reader.GetDecimal(reader.GetOrdinal("PaidAmount")),
                    PaymentStatus = reader.GetString(reader.GetOrdinal("PaymentStatus")),
                    Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                    DoctorId = reader.IsDBNull(reader.GetOrdinal("DoctorId")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("DoctorId")),
                    StaffId = reader.IsDBNull(reader.GetOrdinal("StaffId")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("StaffId")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    BranchName = reader.GetString(reader.GetOrdinal("BranchName")),
                    StartTime = reader.GetTimeSpan(reader.GetOrdinal("StartTime")),
                    EndTime = reader.GetTimeSpan(reader.GetOrdinal("EndTime")),
                    DoctorFirstName = reader.IsDBNull(reader.GetOrdinal("DoctorFirstName")) ? null : reader.GetString(reader.GetOrdinal("DoctorFirstName")),
                    DoctorLastName = reader.IsDBNull(reader.GetOrdinal("DoctorLastName")) ? null : reader.GetString(reader.GetOrdinal("DoctorLastName"))
                });
            }
            return list;
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByBranchAsync(int branchId)
        {
            var list = new List<Appointment>();
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetAppointmentsByBranch", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@BranchId", branchId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Appointment
                {
                    AppointmentId = reader.GetInt32(reader.GetOrdinal("AppointmentId")),
                    PatientId = reader.GetInt32(reader.GetOrdinal("PatientId")),
                    BranchId = reader.GetInt32(reader.GetOrdinal("BranchId")),
                    AppointmentDate = reader.GetDateTime(reader.GetOrdinal("AppointmentDate")),
                    SlotId = reader.GetInt32(reader.GetOrdinal("SlotId")),
                    Status = reader.GetString(reader.GetOrdinal("Status")),
                    TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                    DiscountAmount = reader.GetDecimal(reader.GetOrdinal("DiscountAmount")),
                    PaidAmount = reader.GetDecimal(reader.GetOrdinal("PaidAmount")),
                    PaymentStatus = reader.GetString(reader.GetOrdinal("PaymentStatus")),
                    Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                    DoctorId = reader.IsDBNull(reader.GetOrdinal("DoctorId")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("DoctorId")),
                    StaffId = reader.IsDBNull(reader.GetOrdinal("StaffId")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("StaffId")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    StartTime = reader.GetTimeSpan(reader.GetOrdinal("StartTime")),
                    EndTime = reader.GetTimeSpan(reader.GetOrdinal("EndTime")),
                    PatientFirstName = reader.GetString(reader.GetOrdinal("PatientFirstName")),
                    PatientLastName = reader.GetString(reader.GetOrdinal("PatientLastName")),
                    PatientEmail = reader.GetString(reader.GetOrdinal("PatientEmail")),
                    DoctorFirstName = reader.IsDBNull(reader.GetOrdinal("DoctorFirstName")) ? null : reader.GetString(reader.GetOrdinal("DoctorFirstName")),
                    DoctorLastName = reader.IsDBNull(reader.GetOrdinal("DoctorLastName")) ? null : reader.GetString(reader.GetOrdinal("DoctorLastName"))
                });
            }
            return list;
        }

        public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
        {
            var list = new List<Appointment>();
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetAllAppointments", conn) { CommandType = CommandType.StoredProcedure };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Appointment
                {
                    AppointmentId = reader.GetInt32(reader.GetOrdinal("AppointmentId")),
                    PatientId = reader.GetInt32(reader.GetOrdinal("PatientId")),
                    BranchId = reader.GetInt32(reader.GetOrdinal("BranchId")),
                    AppointmentDate = reader.GetDateTime(reader.GetOrdinal("AppointmentDate")),
                    SlotId = reader.GetInt32(reader.GetOrdinal("SlotId")),
                    Status = reader.GetString(reader.GetOrdinal("Status")),
                    TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                    DiscountAmount = reader.GetDecimal(reader.GetOrdinal("DiscountAmount")),
                    PaidAmount = reader.GetDecimal(reader.GetOrdinal("PaidAmount")),
                    PaymentStatus = reader.GetString(reader.GetOrdinal("PaymentStatus")),
                    Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                    DoctorId = reader.IsDBNull(reader.GetOrdinal("DoctorId")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("DoctorId")),
                    StaffId = reader.IsDBNull(reader.GetOrdinal("StaffId")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("StaffId")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    BranchName = reader.GetString(reader.GetOrdinal("BranchName")),
                    StartTime = reader.GetTimeSpan(reader.GetOrdinal("StartTime")),
                    EndTime = reader.GetTimeSpan(reader.GetOrdinal("EndTime")),
                    PatientFirstName = reader.GetString(reader.GetOrdinal("PatientFirstName")),
                    PatientLastName = reader.GetString(reader.GetOrdinal("PatientLastName")),
                    DoctorFirstName = reader.IsDBNull(reader.GetOrdinal("DoctorFirstName")) ? null : reader.GetString(reader.GetOrdinal("DoctorFirstName")),
                    DoctorLastName = reader.IsDBNull(reader.GetOrdinal("DoctorLastName")) ? null : reader.GetString(reader.GetOrdinal("DoctorLastName")),
                    StaffFirstName = reader.IsDBNull(reader.GetOrdinal("StaffFirstName")) ? null : reader.GetString(reader.GetOrdinal("StaffFirstName")),
                    StaffLastName = reader.IsDBNull(reader.GetOrdinal("StaffLastName")) ? null : reader.GetString(reader.GetOrdinal("StaffLastName"))
                });
            }
            return list;
        }

        public async Task UpdateAppointmentStatusAsync(int appointmentId, string status, int? staffId = null)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_UpdateAppointmentStatus", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@StaffId", (object?)staffId ?? DBNull.Value);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateAppointmentPaymentStatusAsync(int appointmentId, string paymentStatus, decimal paidAmount)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_UpdateAppointmentPaymentStatus", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);
            cmd.Parameters.AddWithValue("@PaymentStatus", paymentStatus);
            cmd.Parameters.AddWithValue("@PaidAmount", paidAmount);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RescheduleAppointmentAsync(int appointmentId, DateTime date, int slotId)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_RescheduleAppointment", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);
            cmd.Parameters.AddWithValue("@AppointmentDate", date);
            cmd.Parameters.AddWithValue("@SlotId", slotId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task CancelAppointmentAsync(int appointmentId)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_CancelAppointment", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        private static Appointment MapAppointment(SqlDataReader reader)
        {
            return new Appointment
            {
                AppointmentId = reader.GetInt32(reader.GetOrdinal("AppointmentId")),
                PatientId = reader.GetInt32(reader.GetOrdinal("PatientId")),
                BranchId = reader.GetInt32(reader.GetOrdinal("BranchId")),
                AppointmentDate = reader.GetDateTime(reader.GetOrdinal("AppointmentDate")),
                SlotId = reader.GetInt32(reader.GetOrdinal("SlotId")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                DiscountAmount = reader.GetDecimal(reader.GetOrdinal("DiscountAmount")),
                PaidAmount = reader.GetDecimal(reader.GetOrdinal("PaidAmount")),
                PaymentStatus = reader.GetString(reader.GetOrdinal("PaymentStatus")),
                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                DoctorId = reader.IsDBNull(reader.GetOrdinal("DoctorId")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("DoctorId")),
                StaffId = reader.IsDBNull(reader.GetOrdinal("StaffId")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("StaffId")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                BranchName = reader.GetString(reader.GetOrdinal("BranchName")),
                BranchAddress = reader.GetString(reader.GetOrdinal("BranchAddress")),
                BranchCity = reader.GetString(reader.GetOrdinal("BranchCity")),
                StartTime = reader.GetTimeSpan(reader.GetOrdinal("StartTime")),
                EndTime = reader.GetTimeSpan(reader.GetOrdinal("EndTime")),
                PatientFirstName = reader.GetString(reader.GetOrdinal("PatientFirstName")),
                PatientLastName = reader.GetString(reader.GetOrdinal("PatientLastName")),
                PatientEmail = reader.GetString(reader.GetOrdinal("PatientEmail")),
                PatientPhone = reader.GetString(reader.GetOrdinal("PatientPhone")),
                DateOfBirth = reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
                Gender = reader.GetString(reader.GetOrdinal("Gender")),
                DoctorFirstName = reader.IsDBNull(reader.GetOrdinal("DoctorFirstName")) ? null : reader.GetString(reader.GetOrdinal("DoctorFirstName")),
                DoctorLastName = reader.IsDBNull(reader.GetOrdinal("DoctorLastName")) ? null : reader.GetString(reader.GetOrdinal("DoctorLastName")),
                StaffFirstName = reader.IsDBNull(reader.GetOrdinal("StaffFirstName")) ? null : reader.GetString(reader.GetOrdinal("StaffFirstName")),
                StaffLastName = reader.IsDBNull(reader.GetOrdinal("StaffLastName")) ? null : reader.GetString(reader.GetOrdinal("StaffLastName"))
            };
        }
    }

    // --- REPORT REPOSITORY ---
    public class ReportRepository : IReportRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;
        public ReportRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<int> CreateReportAsync(Report report)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_CreateReport", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@AppointmentId", report.AppointmentId);
            cmd.Parameters.AddWithValue("@TestId", report.TestId);
            cmd.Parameters.AddWithValue("@ResultValue", (object?)report.ResultValue ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Remarks", (object?)report.Remarks ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UploadedByStaffId", (object?)report.UploadedByStaffId ?? DBNull.Value);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<Report?> GetReportByIdAsync(int reportId)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetReportById", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@ReportId", reportId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReport(reader);
            }
            return null;
        }

        public async Task<IEnumerable<Report>> GetReportsByAppointmentAsync(int appointmentId)
        {
            var list = new List<Report>();
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetReportsByAppointment", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapReport(reader));
            }
            return list;
        }

        private static Report MapReport(SqlDataReader reader)
        {
            return new Report
            {
                ReportId = reader.GetInt32(reader.GetOrdinal("ReportId")),
                AppointmentId = reader.GetInt32(reader.GetOrdinal("AppointmentId")),
                TestId = reader.GetInt32(reader.GetOrdinal("TestId")),
                ResultValue = reader.IsDBNull(reader.GetOrdinal("ResultValue")) ? null : reader.GetString(reader.GetOrdinal("ResultValue")),
                Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks")),
                UploadedByStaffId = reader.IsDBNull(reader.GetOrdinal("UploadedByStaffId")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("UploadedByStaffId")),
                UploadedAt = reader.IsDBNull(reader.GetOrdinal("UploadedAt")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("UploadedAt")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                TestName = reader.GetString(reader.GetOrdinal("TestName")),
                TestCode = reader.GetString(reader.GetOrdinal("TestCode")),
                NormalRange = reader.IsDBNull(reader.GetOrdinal("NormalRange")) ? null : reader.GetString(reader.GetOrdinal("NormalRange")),
                SampleType = reader.GetString(reader.GetOrdinal("SampleType")),
                Preparation = reader.IsDBNull(reader.GetOrdinal("Preparation")) ? null : reader.GetString(reader.GetOrdinal("Preparation")),
                StaffFirstName = reader.IsDBNull(reader.GetOrdinal("StaffFirstName")) ? null : reader.GetString(reader.GetOrdinal("StaffFirstName")),
                StaffLastName = reader.IsDBNull(reader.GetOrdinal("StaffLastName")) ? null : reader.GetString(reader.GetOrdinal("StaffLastName"))
            };
        }
    }

    // --- PAYMENT REPOSITORY ---
    public class PaymentRepository : IPaymentRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;
        public PaymentRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<int> CreatePaymentAsync(Payment payment)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_CreatePayment", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@AppointmentId", payment.AppointmentId);
            cmd.Parameters.AddWithValue("@TransactionId", (object?)payment.TransactionId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Amount", payment.Amount);
            cmd.Parameters.AddWithValue("@PaymentMethod", payment.PaymentMethod);
            cmd.Parameters.AddWithValue("@PaymentStatus", payment.PaymentStatus);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByAppointmentAsync(int appointmentId)
        {
            var list = new List<Payment>();
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetPaymentsByAppointment", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Payment
                {
                    PaymentId = reader.GetInt32(reader.GetOrdinal("PaymentId")),
                    AppointmentId = reader.GetInt32(reader.GetOrdinal("AppointmentId")),
                    TransactionId = reader.IsDBNull(reader.GetOrdinal("TransactionId")) ? null : reader.GetString(reader.GetOrdinal("TransactionId")),
                    Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                    PaymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod")),
                    PaymentStatus = reader.GetString(reader.GetOrdinal("PaymentStatus")),
                    PaymentDate = reader.GetDateTime(reader.GetOrdinal("PaymentDate"))
                });
            }
            return list;
        }

        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
        {
            var list = new List<Payment>();
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetAllPayments", conn) { CommandType = CommandType.StoredProcedure };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Payment
                {
                    PaymentId = reader.GetInt32(reader.GetOrdinal("PaymentId")),
                    AppointmentId = reader.GetInt32(reader.GetOrdinal("AppointmentId")),
                    TransactionId = reader.IsDBNull(reader.GetOrdinal("TransactionId")) ? null : reader.GetString(reader.GetOrdinal("TransactionId")),
                    Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                    PaymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod")),
                    PaymentStatus = reader.GetString(reader.GetOrdinal("PaymentStatus")),
                    PaymentDate = reader.GetDateTime(reader.GetOrdinal("PaymentDate")),
                    PatientId = reader.GetInt32(reader.GetOrdinal("PatientId")),
                    PatientName = reader.GetString(reader.GetOrdinal("PatientName"))
                });
            }
            return list;
        }
    }

    // --- INVOICE REPOSITORY ---
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;
        public InvoiceRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<int> CreateInvoiceAsync(Invoice invoice)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_CreateInvoice", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@AppointmentId", invoice.AppointmentId);
            cmd.Parameters.AddWithValue("@InvoiceNumber", invoice.InvoiceNumber);
            cmd.Parameters.AddWithValue("@TotalAmount", invoice.TotalAmount);
            cmd.Parameters.AddWithValue("@DiscountAmount", invoice.DiscountAmount);
            cmd.Parameters.AddWithValue("@TaxAmount", invoice.TaxAmount);
            cmd.Parameters.AddWithValue("@FinalAmount", invoice.FinalAmount);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<Invoice?> GetInvoiceByAppointmentAsync(int appointmentId)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetInvoiceByAppointment", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Invoice
                {
                    InvoiceId = reader.GetInt32(reader.GetOrdinal("InvoiceId")),
                    AppointmentId = reader.GetInt32(reader.GetOrdinal("AppointmentId")),
                    InvoiceNumber = reader.GetString(reader.GetOrdinal("InvoiceNumber")),
                    TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                    DiscountAmount = reader.GetDecimal(reader.GetOrdinal("DiscountAmount")),
                    TaxAmount = reader.GetDecimal(reader.GetOrdinal("TaxAmount")),
                    FinalAmount = reader.GetDecimal(reader.GetOrdinal("FinalAmount")),
                    GeneratedAt = reader.GetDateTime(reader.GetOrdinal("GeneratedAt"))
                };
            }
            return null;
        }
    }

    // --- NOTIFICATION REPOSITORY ---
    public class NotificationRepository : INotificationRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;
        public NotificationRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<int> CreateNotificationAsync(Notification notification)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_CreateNotification", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@UserId", notification.UserId);
            cmd.Parameters.AddWithValue("@Message", notification.Message);
            cmd.Parameters.AddWithValue("@Type", notification.Type);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<IEnumerable<Notification>> GetNotificationsByUserAsync(int userId)
        {
            var list = new List<Notification>();
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetNotificationsByUser", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@UserId", userId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Notification
                {
                    NotificationId = reader.GetInt32(reader.GetOrdinal("NotificationId")),
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    Message = reader.GetString(reader.GetOrdinal("Message")),
                    Type = reader.GetString(reader.GetOrdinal("Type")),
                    IsRead = reader.GetBoolean(reader.GetOrdinal("IsRead")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                });
            }
            return list;
        }

        public async Task MarkNotificationReadAsync(int notificationId)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_MarkNotificationRead", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@NotificationId", notificationId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }

    // --- FEEDBACK REPOSITORY ---
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;
        public FeedbackRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<int> CreateFeedbackAsync(Feedback feedback)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_CreateFeedback", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@PatientId", feedback.PatientId);
            cmd.Parameters.AddWithValue("@Rating", feedback.Rating);
            cmd.Parameters.AddWithValue("@Comments", (object?)feedback.Comments ?? DBNull.Value);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<IEnumerable<Feedback>> GetAllFeedbackAsync()
        {
            var list = new List<Feedback>();
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetAllFeedback", conn) { CommandType = CommandType.StoredProcedure };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Feedback
                {
                    FeedbackId = reader.GetInt32(reader.GetOrdinal("FeedbackId")),
                    PatientId = reader.GetInt32(reader.GetOrdinal("PatientId")),
                    Rating = reader.GetInt32(reader.GetOrdinal("Rating")),
                    Comments = reader.IsDBNull(reader.GetOrdinal("Comments")) ? null : reader.GetString(reader.GetOrdinal("Comments")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    PatientName = reader.GetString(reader.GetOrdinal("PatientName"))
                });
            }
            return list;
        }
    }

    // --- AUDIT LOG REPOSITORY ---
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;
        public AuditLogRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task CreateAuditLogAsync(AuditLog log)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_CreateAuditLog", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@UserId", (object?)log.UserId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Action", log.Action);
            cmd.Parameters.AddWithValue("@TableName", log.TableName);
            cmd.Parameters.AddWithValue("@RecordId", (object?)log.RecordId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@OldValues", (object?)log.OldValues ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NewValues", (object?)log.NewValues ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IpAddress", (object?)log.IpAddress ?? DBNull.Value);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync()
        {
            var list = new List<AuditLog>();
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = new SqlCommand("dbo.sp_GetAuditLogs", conn) { CommandType = CommandType.StoredProcedure };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new AuditLog
                {
                    LogId = reader.GetInt32(reader.GetOrdinal("LogId")),
                    UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("UserId")),
                    Action = reader.GetString(reader.GetOrdinal("Action")),
                    TableName = reader.GetString(reader.GetOrdinal("TableName")),
                    RecordId = reader.IsDBNull(reader.GetOrdinal("RecordId")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("RecordId")),
                    OldValues = reader.IsDBNull(reader.GetOrdinal("OldValues")) ? null : reader.GetString(reader.GetOrdinal("OldValues")),
                    NewValues = reader.IsDBNull(reader.GetOrdinal("NewValues")) ? null : reader.GetString(reader.GetOrdinal("NewValues")),
                    Timestamp = reader.GetDateTime(reader.GetOrdinal("Timestamp")),
                    IpAddress = reader.IsDBNull(reader.GetOrdinal("IpAddress")) ? null : reader.GetString(reader.GetOrdinal("IpAddress")),
                    UserEmail = reader.IsDBNull(reader.GetOrdinal("UserEmail")) ? null : reader.GetString(reader.GetOrdinal("UserEmail")),
                    UserName = reader.IsDBNull(reader.GetOrdinal("UserName")) ? null : reader.GetString(reader.GetOrdinal("UserName"))
                });
            }
            return list;
        }
    }

    // --- DASHBOARD REPOSITORY ---
    public class DashboardRepository : IDashboardRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;
        public DashboardRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<AdminDashboardStatsDto> GetAdminDashboardStatsAsync(int? branchId, int? year, int? month, DateTime? startDate, DateTime? endDate)
        {
            var stats = new AdminDashboardStatsDto();
            using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync();

            DateTime? filterStart = null;
            DateTime? filterEnd = null;

            if (startDate.HasValue) filterStart = startDate.Value.Date;
            if (endDate.HasValue) filterEnd = endDate.Value.Date;

            if (!filterStart.HasValue && !filterEnd.HasValue)
            {
                if (year.HasValue)
                {
                    if (month.HasValue)
                    {
                        filterStart = new DateTime(year.Value, month.Value, 1);
                        filterEnd = filterStart.Value.AddMonths(1).AddDays(-1);
                    }
                    else
                    {
                        filterStart = new DateTime(year.Value, 1, 1);
                        filterEnd = new DateTime(year.Value, 12, 31);
                    }
                }
            }

            string apptWhere = " WHERE 1=1";
            var parameters = new List<SqlParameter>();

            if (branchId.HasValue)
            {
                apptWhere += " AND a.BranchId = @BranchId";
                parameters.Add(new SqlParameter("@BranchId", branchId.Value));
            }
            if (filterStart.HasValue)
            {
                apptWhere += " AND a.AppointmentDate >= @StartDate";
                parameters.Add(new SqlParameter("@StartDate", filterStart.Value));
            }
            if (filterEnd.HasValue)
            {
                apptWhere += " AND a.AppointmentDate <= @EndDate";
                parameters.Add(new SqlParameter("@EndDate", filterEnd.Value));
            }

            void AddParams(SqlCommand cmd)
            {
                foreach (var p in parameters)
                {
                    cmd.Parameters.Add(new SqlParameter(p.ParameterName, p.Value));
                }
            }

            // 1. Total Patients
            string totalPatientsSql = $"SELECT COUNT(DISTINCT a.PatientId) FROM dbo.Appointments a {apptWhere}";
            using (var cmd = new SqlCommand(totalPatientsSql, conn))
            {
                AddParams(cmd);
                stats.TotalPatients = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }

            // 2. Total Appointments
            string totalApptsSql = $"SELECT COUNT(*) FROM dbo.Appointments a {apptWhere}";
            using (var cmd = new SqlCommand(totalApptsSql, conn))
            {
                AddParams(cmd);
                stats.TotalAppointments = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }

            // 3. Today's Bookings
            string todayBookingsSql = $"SELECT COUNT(*) FROM dbo.Appointments a WHERE a.AppointmentDate = CAST(GETDATE() AS DATE)";
            if (branchId.HasValue)
            {
                todayBookingsSql += " AND a.BranchId = @BranchId";
            }
            using (var cmd = new SqlCommand(todayBookingsSql, conn))
            {
                if (branchId.HasValue) cmd.Parameters.AddWithValue("@BranchId", branchId.Value);
                stats.TodayBookings = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }

            // 4. Pending Reports
            string pendingSql = $"SELECT COUNT(*) FROM dbo.Appointments a WHERE a.Status IN ('Pending', 'Confirmed', 'SampleCollected')";
            if (branchId.HasValue)
            {
                pendingSql += " AND a.BranchId = @BranchId";
            }
            using (var cmd = new SqlCommand(pendingSql, conn))
            {
                if (branchId.HasValue) cmd.Parameters.AddWithValue("@BranchId", branchId.Value);
                stats.PendingReports = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }

            // 5. Total Collection
            string revenueSql = $"SELECT ISNULL(SUM(a.PaidAmount), 0) FROM dbo.Appointments a {apptWhere} AND a.PaymentStatus = 'Paid'";
            using (var cmd = new SqlCommand(revenueSql, conn))
            {
                AddParams(cmd);
                stats.TotalRevenue = Convert.ToDecimal(await cmd.ExecuteScalarAsync());
            }

            // 6. New Patients vs Returning Patients
            string newPatientsSql = "";
            string returningPatientsSql = "";
            if (filterStart.HasValue || filterEnd.HasValue || branchId.HasValue)
            {
                newPatientsSql = @"
                    SELECT COUNT(*) FROM (
                        SELECT PatientId, MIN(AppointmentDate) as FirstAppt
                        FROM dbo.Appointments a
                        GROUP BY PatientId
                    ) g
                    INNER JOIN dbo.Appointments a ON g.PatientId = a.PatientId
                    " + apptWhere + @" AND g.FirstAppt = a.AppointmentDate";

                returningPatientsSql = @"
                    SELECT COUNT(DISTINCT a.PatientId)
                    FROM dbo.Appointments a
                    " + apptWhere + @"
                    AND EXISTS (
                        SELECT 1 FROM dbo.Appointments a2 
                        WHERE a2.PatientId = a.PatientId 
                        AND a2.AppointmentDate < a.AppointmentDate
                    )";
            }
            else
            {
                newPatientsSql = "SELECT COUNT(*) FROM dbo.Patients";
                returningPatientsSql = @"
                    SELECT COUNT(*) FROM (
                        SELECT PatientId FROM dbo.Appointments GROUP BY PatientId HAVING COUNT(*) > 1
                    ) g";
            }

            using (var cmd = new SqlCommand(newPatientsSql, conn))
            {
                AddParams(cmd);
                stats.NewPatients = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }

            using (var cmd = new SqlCommand(returningPatientsSql, conn))
            {
                AddParams(cmd);
                stats.ReturningPatients = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }

            stats.AverageRevenuePerPatient = stats.TotalPatients > 0 ? stats.TotalRevenue / stats.TotalPatients : 0;

            // 7. Popular Tests
            string popularSql = $@"
                SELECT TOP 5 t.Name AS TestName, COUNT(at.TestId) AS BookingCount, SUM(at.Price) AS RevenueGenerated
                FROM dbo.AppointmentTests at
                INNER JOIN dbo.Tests t ON at.TestId = t.TestId
                INNER JOIN dbo.Appointments a ON at.AppointmentId = a.AppointmentId
                {apptWhere}
                GROUP BY t.Name
                ORDER BY BookingCount DESC";
            using (var cmd = new SqlCommand(popularSql, conn))
            {
                AddParams(cmd);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        stats.PopularTests.Add(new PopularTestStatsDto
                        {
                            TestName = reader.GetString(reader.GetOrdinal("TestName")),
                            BookingCount = reader.GetInt32(reader.GetOrdinal("BookingCount")),
                            RevenueGenerated = reader.GetDecimal(reader.GetOrdinal("RevenueGenerated"))
                        });
                    }
                }
            }

            // 8. Monthly Collection Trend
            string monthlyTrendSql = $@"
                SELECT TOP 6 
                    FORMAT(a.AppointmentDate, 'yyyy-MM') AS MonthName, 
                    SUM(a.PaidAmount) AS Revenue
                FROM dbo.Appointments a
                {apptWhere} AND a.PaymentStatus = 'Paid'
                GROUP BY FORMAT(a.AppointmentDate, 'yyyy-MM')
                ORDER BY MonthName ASC";
            using (var cmd = new SqlCommand(monthlyTrendSql, conn))
            {
                AddParams(cmd);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        stats.MonthlyRevenue.Add(new MonthlyRevenueStatsDto
                        {
                            MonthName = reader.GetString(reader.GetOrdinal("MonthName")),
                            Revenue = reader.GetDecimal(reader.GetOrdinal("Revenue"))
                        });
                    }
                }
            }

            // 9. Monthly Patient Growth
            string monthlyPatientsSql = $@"
                SELECT TOP 6 
                    FORMAT(a.AppointmentDate, 'yyyy-MM') AS MonthName, 
                    COUNT(DISTINCT a.PatientId) AS PatientsCount
                FROM dbo.Appointments a
                {apptWhere}
                GROUP BY FORMAT(a.AppointmentDate, 'yyyy-MM')
                ORDER BY MonthName ASC";
            using (var cmd = new SqlCommand(monthlyPatientsSql, conn))
            {
                AddParams(cmd);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        stats.MonthlyPatients.Add(new MonthlyPatientsStatsDto
                        {
                            MonthName = reader.GetString(reader.GetOrdinal("MonthName")),
                            PatientsCount = reader.GetInt32(reader.GetOrdinal("PatientsCount"))
                        });
                    }
                }
            }

            // 10. Branch-wise Collection & Patient Comparison
            string branchCompSql = $@"
                SELECT b.Name AS BranchName, SUM(a.PaidAmount) AS Collection, COUNT(DISTINCT a.PatientId) AS PatientsCount
                FROM dbo.Appointments a
                INNER JOIN dbo.Branches b ON a.BranchId = b.BranchId
                {apptWhere}
                GROUP BY b.Name";
            using (var cmd = new SqlCommand(branchCompSql, conn))
            {
                AddParams(cmd);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        stats.BranchCollections.Add(new BranchCollectionStatsDto
                        {
                            BranchName = reader.GetString(reader.GetOrdinal("BranchName")),
                            Collection = reader.GetDecimal(reader.GetOrdinal("Collection"))
                        });
                        stats.BranchPatients.Add(new BranchPatientStatsDto
                        {
                            BranchName = reader.GetString(reader.GetOrdinal("BranchName")),
                            PatientsCount = reader.GetInt32(reader.GetOrdinal("PatientsCount"))
                        });
                    }
                }
            }

            // 11. Recent Activity
            string recentSql = $@"
                SELECT TOP 10 
                    a.AppointmentId, 
                    a.AppointmentDate, 
                    a.Status, 
                    u.FirstName + ' ' + u.LastName AS PatientName,
                    a.TotalAmount
                FROM dbo.Appointments a
                INNER JOIN dbo.Patients p ON a.PatientId = p.PatientId
                INNER JOIN dbo.Users u ON p.UserId = u.UserId
                {apptWhere}
                ORDER BY a.CreatedAt DESC";
            using (var cmd = new SqlCommand(recentSql, conn))
            {
                AddParams(cmd);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        stats.RecentActivity.Add(new RecentActivityStatsDto
                        {
                            AppointmentId = reader.GetInt32(reader.GetOrdinal("AppointmentId")),
                            AppointmentDate = reader.GetDateTime(reader.GetOrdinal("AppointmentDate")),
                            Status = reader.GetString(reader.GetOrdinal("Status")),
                            PatientName = reader.GetString(reader.GetOrdinal("PatientName")),
                            TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount"))
                        });
                    }
                }
            }

            return stats;
        }
    }
}
