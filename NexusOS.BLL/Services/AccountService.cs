using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NexusOS.BLL.Interfaces;
using NexusOS.DAL.Models;
using NexusOS.MB;
using NexusOS.Util;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace NexusOS.BLL.Services
{
    public class AccountService : IAccountService
    {
        #region Infrastructure

        private readonly NexusOsContext _context; // Dùng để truy cập vào DbContext
        private readonly IConfiguration _config; // Dùng để đọc cấu hình ứng dụng
        private readonly IdentityOptions _options; // Dùng để lấy thiết lập Identity
        private readonly IStringLocalizer _localizer; // Dùng để đa ngôn ngữ hóa thông báo
        private static readonly ConcurrentDictionary<string, object> otpStore
            = new ConcurrentDictionary<string, object>(); // Dùng để lưu trữ mã OTP tạm thời

        public AccountService(NexusOsContext context, IConfiguration config, IStringLocalizer localizer, IOptions<IdentityOptions> options)
        {
            _context = context;
            _config = config;
            _localizer = localizer;
            _options = options.Value;
        }

        #endregion

        #region Default Operations

        #endregion

        #region Custom Operations

        public async Task<APIResults<string>> Auth(UserModel request)
        {
            var username = DataHelpers.GetString(request.Username);
            var password = DataHelpers.GetString(request.Password);

            var user = await _context.Users.AsNoTracking() // Tắt cơ chế "theo dõi thay đổi" (Change Tracking) của Entity Framework
                .FirstOrDefaultAsync(s => !string.IsNullOrWhiteSpace(s.Username) && s.Username == username);
            if (user == null)
                return APIResults<string>.Failure(_localizer[Messages.InvalidUsernameOrPassword]); // Luôn trả ra message chung chung để tránh hacker biết được thông tin chính xác

            var checkPassword = PasswordHasher.VerifyPassword(password, DataHelpers.GetString(user.PasswordHash));
            if (checkPassword == false)
                return APIResults<string>.Failure(_localizer[Messages.InvalidUsernameOrPassword]); // Luôn trả ra message chung chung để tránh hacker biết được thông tin chính xác

            //var employee = await _context.Employees.FirstOrDefaultAsync(s => s.Id == DataHelpers.GetGuid(user.EmployeeId));

            //var id = employee != null ? employee.Id : Guid.Empty;
            //var employeeName = employee != null ? employee.EmployeeName : string.Empty;

            var id = Guid.Empty;
            var employeeName = string.Empty;

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, DataHelpers.GetString(user.Username)),
                new Claim(ClaimTypes.GivenName, DataHelpers.GetString(employeeName)),
                new Claim("UserID", DataHelpers.GetString(user.Id.ToString())),
                new Claim("OwnerID", DataHelpers.GetString(id.ToString())),
            };

            #region Add roleNames to claim Role

            //var listUserRole = await _context.UserRoles
            //    .Where(s => s.UserId == user.Id)
            //    .Join(_context.Roles,
            //        ur => ur.RoleId,
            //        r => r.Id,
            //        (ur, r) => r.RoleName)
            //    .ToListAsync();

            //claims.AddRange(listUserRole.Select(roleName => new Claim(ClaimTypes.Role, DataHelpers.GetString(roleName))));

            #endregion

            var tokenKey = _config["Tokens:Key"];
            var tokenIssuer = _config["Tokens:Issuer"];

            if (string.IsNullOrEmpty(tokenKey))
                return APIResults<string>.Failure(_localizer[Messages.TokenKeyNotConfigured]);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            DateTime expirationTime = request.Remember
                ? DateTime.UtcNow.AddYears(1) : DateTime.UtcNow.AddHours(1); // UtcNow để không lỗi khi server khác timezone

            var token = new JwtSecurityToken(tokenIssuer, tokenIssuer, claims,
               expires: expirationTime, signingCredentials: creds);

            return token != null
                ? APIResults<string>.Success(new JwtSecurityTokenHandler().WriteToken(token), _localizer[Messages.AuthSuccess])
                : APIResults<string>.Failure(_localizer[Messages.AuthFailure]);
        }

        public async Task<APIResults<bool>> ResetPass(UserModel request)
        {
            var username = DataHelpers.GetString(request.Username);
            var phoneNumber = DataHelpers.GetString(request.PhoneNumber);
            var mail = DataHelpers.GetString(request.Email);
            var password = DataHelpers.GetString(request.Password);
            var passwordHashed = PasswordHasher.HashPassword(password);

            var validateMessage = ValidateOtp(DataHelpers.GetString(request.Email), DataHelpers.GetString(request.Otp));
            if (!string.IsNullOrEmpty(validateMessage))
                return APIResults<bool>.Failure(validateMessage);

            var user = await _context.Users.AsNoTracking() // Tắt cơ chế "theo dõi thay đổi" (Change Tracking) của Entity Framework
                .FirstOrDefaultAsync(s => s.Username == username
               && s.PhoneNumber == phoneNumber && s.Email == mail);

            if (user == null)
                return APIResults<bool>.Failure(_localizer[Messages.NotFoundUpdate]);

            var validationResult = ValidatePass(password);
            if (validationResult.Any())
            {
                var error = string.Join(", ", validationResult.Select(e =>
                    e.Description != null && e.Description.Length > 0
                    ? $"{e.Code}|{string.Join("|", e.Description)}" : e.Code));

                return APIResults<bool>.Failure(error);
            }

            user.PasswordHash = passwordHashed;

            var result = await _context.SaveChangesAsync();

            return result > 0
                ? APIResults<bool>.Success(true, _localizer[Messages.ResetPasswordSuccess])
                : APIResults<bool>.Failure(_localizer[Messages.ResetPasswordFailure]);
        }

        public async Task<APIResults<bool>> SendOTP(MailModel mail)
        {
            var otpString = GenerateOtp(DataHelpers.GetString(mail.To));
            mail.Subject = "SendOTP";
            mail.Body = otpString;

            var result = await MailHelpers.SendMail(mail);

            return result
                ? APIResults<bool>.Success(true, _localizer[Messages.SendMailSuccess])
                : APIResults<bool>.Failure(_localizer[Messages.SendMailFailure]);
        }

        private string ValidateOtp(string email, string otp)
        {
            if (!otpStore.TryGetValue(email, out var entryObj))
                return _localizer[Messages.OTPNotFound];

            var entry = (dynamic)entryObj;

            // Kiểm tra hết hạn
            if (DateTime.UtcNow > entry.ExpiresAt)
            {
                otpStore.TryRemove(email, out _);
                return _localizer[Messages.OTPExpired];
            }

            // Kiểm tra số lần thử còn lại
            if (entry.AttemptsLeft <= 0)
            {
                otpStore.TryRemove(email, out _);
                return _localizer[Messages.OTPNoAttemptsLeft];
            }

            // Hash OTP nhập để so sánh
            using var sha = SHA256.Create();
            var hashInput = sha.ComputeHash(Encoding.UTF8.GetBytes(otp + entry.Salt));
            var hashInputString = Convert.ToBase64String(hashInput);

            if (hashInputString != entry.Hashed)
            {
                entry.AttemptsLeft--;
                otpStore[email] = entry; // cập nhật số lần thử
                return _localizer[Messages.OTPIncorrect];
            }

            // OTP đúng → remove khỏi store
            otpStore.TryRemove(email, out _);

            return string.Empty;
        }

        private string GenerateOtp(string email)
        {
            // 1. Sinh OTP 6 chữ số an toàn
            byte[] rngBytes = new byte[4];
            RandomNumberGenerator.Fill(rngBytes);
            var otp = BitConverter.ToUInt32(rngBytes, 0) % 1000000;
            var otpString = otp.ToString("D6");

            // 2. Tạo salt + hash OTP
            var salt = Guid.NewGuid().ToString();
            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(otpString + salt));
            var hashed = Convert.ToBase64String(hashBytes);

            // 3. Lưu vào store tạm
            otpStore[email] = new
            {
                Hashed = hashed,
                Salt = salt,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                AttemptsLeft = 3
            };

            return otpString;
        }

        public List<IdentityError> ValidatePass(string password)
        {
            var errors = new List<IdentityError>();

            if (password.Length < _options.Password.RequiredLength)
                errors.Add(new IdentityError
                {
                    Code = _localizer[Messages.RequiredLength],
                    Description = _options.Password.RequiredLength.ToString()
                });

            if (_options.Password.RequireDigit && !password.Any(char.IsDigit))
                errors.Add(new IdentityError { Code = _localizer[Messages.RequireDigit] });

            if (_options.Password.RequireLowercase && !password.Any(char.IsLower))
                errors.Add(new IdentityError { Code = _localizer[Messages.RequireLowercase] });

            if (_options.Password.RequireUppercase && !password.Any(char.IsUpper))
                errors.Add(new IdentityError { Code = _localizer[Messages.RequireUppercase] });

            if (_options.Password.RequireNonAlphanumeric && password.All(char.IsLetterOrDigit))
                errors.Add(new IdentityError { Code = _localizer[Messages.RequireNonAlphanumeric] });

            if (password.Distinct().Count() < _options.Password.RequiredUniqueChars)
                errors.Add(new IdentityError
                {
                    Code = _localizer[Messages.RequiredUniqueChars],
                    Description = _options.Password.RequiredUniqueChars.ToString()
                });

            return errors;
        }

        #endregion
    }
}
