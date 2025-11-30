using DataAccess.Generic;
using DnsClient.Internal;
using Entities.DataContext;
using Entities.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace DataAccess.SupportServices
{
    public interface IAuthenticationService
    {
        string EncryptationSHA256(string text);
        string GenerateAccessJwt(User user);
        Task<string> GenerateSecureRandomToken(User user, TimeSpan lifetime, int length = 32);
        Task<SecureRandomToken?> ValidateAsync(string rawToken);
    }
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ISqlGenericRepository<SecureRandomToken, ServiceDbContext> _tokenSqlGenericRepository;
        private readonly IPasswordHasher<User> _hasher;
        private readonly ILogger<AuthenticationService> _logger;
        public AuthenticationService(ISqlGenericRepository<SecureRandomToken, ServiceDbContext> tokenSqlGenericRepository, IPasswordHasher<User> hasher, ILogger<AuthenticationService> logger)
        {
            _tokenSqlGenericRepository = tokenSqlGenericRepository;
            _hasher = hasher;
            _logger = logger;
        }

        public string EncryptationSHA256(string text)
        {
            SHA256 sha256Hash = SHA256.Create();
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(text));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }

        public string GenerateAccessJwt(User user)
        {
            Claim[] userClaims = new[] {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Surname, user.Lastname),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.Name)
            };
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY_ACCESS")));
            SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
            var jwtConfig = new JwtSecurityToken(claims:userClaims, expires:DateTime.UtcNow.AddMinutes(60), signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(jwtConfig);
        }

        public async Task<string> GenerateSecureRandomToken(User user, TimeSpan lifetime, int length = 64)
        {
            try
            {
                byte[] bytes = RandomNumberGenerator.GetBytes(length);
                string base64Token = Convert.ToBase64String(bytes);
                SecureRandomToken entity = new SecureRandomToken
                {
                    TokenHash = _hasher.HashPassword(user, base64Token),
                    ExpiredDate = DateTime.UtcNow.Add(lifetime),
                    UserId = user.Id,
                    CreatedDate = DateTime.UtcNow,
                    Used = false
                };
                int id = (await _tokenSqlGenericRepository.CreateAsync(entity)).Value;
                return $"{id}-{base64Token}";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return string.Empty;
            }
        }

        public async Task<SecureRandomToken?> ValidateAsync(string rawToken)
        {
            int dashIndex = rawToken.IndexOf('-');
            string idPart = rawToken.Substring(0, dashIndex);
            string tokenPart = rawToken.Substring(dashIndex + 1);
            SecureRandomToken tokenEntity = await _tokenSqlGenericRepository.GetByIdAsync(t => t.Id == Convert.ToInt32(idPart) && !t.Used && t.ExpiredDate > DateTime.UtcNow, t => t.User);
            if (tokenEntity == null) { return null; }
            User user = tokenEntity.User!;
            var result = _hasher.VerifyHashedPassword(user, tokenEntity.TokenHash, tokenPart);
            if (result != PasswordVerificationResult.Failed)
            {
                tokenEntity.Used = true;
                await _tokenSqlGenericRepository.UpdateByEntityAsync(tokenEntity);
                return tokenEntity;
            }
            return null;
        }
    }
}