using System;
using System.Security.Cryptography;
using System.Text;
using Leb.Core.Interfaces;

namespace Leb.Infrastructure.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        public void HashPassword(string password, out string passwordHash, out string salt)
        {
            salt = Guid.NewGuid().ToString("N");
            passwordHash = ComputeHash(password, salt);
        }

        public bool VerifyPassword(string password, string passwordHash, string salt)
        {
            var hash = ComputeHash(password, salt);
            return string.Equals(hash, passwordHash, StringComparison.OrdinalIgnoreCase);
        }

        private static string ComputeHash(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            var combined = password + salt;
            var bytes = Encoding.UTF8.GetBytes(combined);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hashBytes).ToLower();
        }
    }
}
