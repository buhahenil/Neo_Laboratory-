using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Leb.Core.Entities;
using Leb.Core.Interfaces;

namespace Leb.Infrastructure.Services
{
    public class JWTService : IJWTService
    {
        private readonly IConfiguration _configuration;
        private readonly string _key;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiryMinutes;

        public JWTService(IConfiguration configuration)
        {
            _configuration = configuration;
            _key = _configuration["Jwt:Key"] ?? "SecretKeyForLabManagementSystemSecurityKeyNeededToBeLongAndComplex";
            _issuer = _configuration["Jwt:Issuer"] ?? "LabSystemAPI";
            _audience = _configuration["Jwt:Audience"] ?? "LabSystemClient";
            _expiryMinutes = Convert.ToInt32(_configuration["Jwt:ExpiryMinutes"] ?? "120");
        }

        public string GenerateToken(User user, int? patientId = null, int? staffId = null, int? doctorId = null, int? branchId = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var keyBytes = Encoding.UTF8.GetBytes(_key);

            var claims = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.RoleName),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName),
            });

            if (patientId.HasValue) claims.AddClaim(new Claim("PatientId", patientId.Value.ToString()));
            if (staffId.HasValue) claims.AddClaim(new Claim("StaffId", staffId.Value.ToString()));
            if (doctorId.HasValue) claims.AddClaim(new Claim("DoctorId", doctorId.Value.ToString()));
            if (branchId.HasValue) claims.AddClaim(new Claim("BranchId", branchId.Value.ToString()));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddMinutes(_expiryMinutes),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public bool ValidateToken(string token, out string email, out string role)
        {
            email = string.Empty;
            role = string.Empty;
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var keyBytes = Encoding.UTF8.GetBytes(_key);
                
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                email = jwtToken.Payload[ClaimTypes.Email]?.ToString() ?? string.Empty;
                role = jwtToken.Payload[ClaimTypes.Role]?.ToString() ?? string.Empty;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
