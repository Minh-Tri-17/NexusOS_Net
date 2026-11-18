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
        private readonly IStringLocalizer<SharedResource> _localizer; // Dùng để đa ngôn ngữ hóa thông báo
        private readonly MailHelpers _mailHelpers; // Dùng để quản lý việc gửi email
        private readonly ICurrentUserService _currentUser; // Dùng để lấy thông tin người dùng hiện tại
        private readonly IPermissionService _permissionService; // Dùng để lấy danh sách quyền của người dùng
        private static readonly ConcurrentDictionary<string, OtpEntry> otpStore
            = new ConcurrentDictionary<string, OtpEntry>(); // Dùng để lưu trữ mã OTP tạm thời

        public AccountService(NexusOsContext context, IConfiguration config,
            IStringLocalizer<SharedResource> localizer, IOptions<IdentityOptions> options, MailHelpers mailHelpers,
            ICurrentUserService currentUser, IPermissionService permissionService)
        {
            _context = context;
            _config = config;
            _localizer = localizer;
            _options = options.Value;
            _mailHelpers = mailHelpers;
            _currentUser = currentUser;
            _permissionService = permissionService;
        }

        #endregion

        #region Default Operations

        #endregion

        #region Custom Operations

        public async Task<APIResults<string>> Auth(UserModel request)
        {
            var username = DataHelpers.GetString(request.Username);
            var password = DataHelpers.GetString(request.Password);

            var user = await _context.Users
                .FirstOrDefaultAsync(s => s.IsDelete != true && s.UserStatus == UserStatus.Active.ToString()
                    && !string.IsNullOrWhiteSpace(s.Username) && (s.Username == username || s.Email == username));

            #region Verify user credentials and lockout

            if (user == null)
                return APIResults<string>.Failure(_localizer[Messages.InvalidUsernameOrPassword]); // Luôn trả ra message chung chung để tránh hacker biết được thông tin chính xác

            if (user.LockoutEnd.HasValue)
            {
                if (user.LockoutEnd.Value > DateTime.UtcNow)
                {
                    var remainingMinutes = (int)Math.Ceiling((user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
                    return APIResults<string>.Failure(_localizer[Messages.AccountLockedWithTime, remainingMinutes]);
                }

                user.LockoutEnd = null;
                user.FailedLoginCount = 0;
            }

            var isUserModified = _context.Entry(user).State == EntityState.Modified; // Kiểm tra xem entity có bị thay đổi kể từ lúc lấy từ DB lên

            var checkPassword = PasswordHasher.VerifyPassword(password, DataHelpers.GetString(user.PasswordHash));
            if (checkPassword == false)
            {
                user.FailedLoginCount = (user.FailedLoginCount ?? 0) + 1;

                if (user.FailedLoginCount >= 5)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(10);
                    await _context.SaveChangesAsync();

                    return APIResults<string>.Failure(_localizer[Messages.AccountLockedWithTime, 10]);
                }

                await _context.SaveChangesAsync();

                return APIResults<string>.Failure(_localizer[Messages.InvalidUsernameOrPassword]); // Luôn trả ra message chung chung để tránh hacker biết được thông tin chính xác
            }
            else if (user.FailedLoginCount > 0 || user.LockoutEnd.HasValue || isUserModified)
            {
                user.FailedLoginCount = 0;
                user.LockoutEnd = null;
                await _context.SaveChangesAsync();
            }

            #endregion

            var employee = await _context.Employees.FirstOrDefaultAsync(s => s.IsDelete != true
                && s.Id == DataHelpers.GetGuid(user.EmployeeId));

            var id = employee != null ? employee.Id : Guid.Empty;
            var employeeName = employee != null ? employee.EmployeeName : string.Empty;

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
                new Claim(ClaimTypes.Name, DataHelpers.GetString(user.Username)),
                new Claim(ClaimTypes.GivenName, DataHelpers.GetString(employeeName)),
                new Claim("UserID", DataHelpers.GetString(user.Id.ToString())),
                new Claim("OwnerID", DataHelpers.GetString(id.ToString())),
            };

            #region Add roleIds to claim Role

            var roleIds = await _context.UserRoles
                .AsNoTracking()
                .Where(s => s.IsDelete != true && s.IsActive != false && s.UserId == user.Id)
                .Select(s => s.RoleId)
                .ToListAsync();

            claims.AddRange(roleIds.Select(roleId => new Claim(ClaimTypes.Role, DataHelpers.GetString(roleId))));

            #endregion

            var tokenKey = _config["Tokens:Key"];
            var tokenIssuer = _config["Tokens:Issuer"];

            if (string.IsNullOrEmpty(tokenKey))
                return APIResults<string>.Failure(_localizer[Messages.TokenKeyNotConfigured]);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            DateTime expirationTime = request.Remember
                ? DateTime.UtcNow.AddMonths(1) : DateTime.UtcNow.AddDays(1); // UtcNow để không lỗi khi server khác timezone

            var token = new JwtSecurityToken(tokenIssuer, tokenIssuer, claims,
               expires: expirationTime, signingCredentials: creds);

            return token != null
                ? APIResults<string>.Success(new JwtSecurityTokenHandler().WriteToken(token), _localizer[Messages.AuthSuccess])
                : APIResults<string>.Failure(_localizer[Messages.AuthFailure]);
        }

        public async Task<APIResults<bool>> ResetPass(UserModel request)
        {
            var mail = DataHelpers.GetString(request.Email).Trim().ToLowerInvariant();
            var password = DataHelpers.GetString(request.Password);

            if (!otpStore.TryGetValue(mail, out var otpEntry) || !otpEntry.IsVerified)
                return APIResults<bool>.Failure(_localizer[Messages.OTPNotFound]);

            if (DateTime.UtcNow > otpEntry.ExpiresAt)
            {
                otpStore.TryRemove(mail, out _);
                return APIResults<bool>.Failure(_localizer[Messages.OTPExpired]);
            }

            var validationResult = ValidatePass(password);
            if (validationResult.Any())
            {
                var error = string.Join(", ", validationResult.Select(e =>
                    e.Description != null && e.Description.Length > 0
                    ? $"{e.Code}|{string.Join("|", e.Description)}" : e.Code));

                return APIResults<bool>.Failure(error);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(s => s.IsDelete != true && s.Email == mail);

            if (user == null)
                return APIResults<bool>.Failure(_localizer[Messages.NotFoundUpdate]);

            user.PasswordHash = PasswordHasher.HashPassword(password);

            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                otpStore.TryRemove(mail, out _);
                return APIResults<bool>.Success(true, _localizer[Messages.ResetPasswordSuccess]);
            }

            return APIResults<bool>.Failure(_localizer[Messages.ResetPasswordFailure]);
        }

        public async Task<APIResults<bool>> ValidateOtp(OTPModel request)
        {
            var mail = DataHelpers.GetString(request.Email).Trim().ToLowerInvariant();
            var otp = DataHelpers.GetString(request.Otp);

            var validateMessage = ValidateOtp(mail, otp);
            if (!string.IsNullOrEmpty(validateMessage))
                return APIResults<bool>.Failure(validateMessage);

            return APIResults<bool>.Success(true, _localizer[Messages.OTPCorrect]);
        }

        private string ValidateOtp(string email, string otp)
        {
            if (!otpStore.TryGetValue(email, out var entry) || entry.IsVerified)
                return _localizer[Messages.OTPNotFound];

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
                return _localizer[Messages.OTPIncorrect, entry.AttemptsLeft];
            }

            // OTP đúng → đánh dấu đã xác thực và gia hạn 5 phút để đổi mật khẩu
            entry.IsVerified = true;
            entry.ExpiresAt = DateTime.UtcNow.AddMinutes(5);
            otpStore[email] = entry;

            return string.Empty;
        }

        public async Task<APIResults<bool>> SendOTP(MailModel mail)
        {
            var email = DataHelpers.GetString(mail.To).Trim().ToLowerInvariant();
            var otpString = GenerateOtp(email);
            mail.Subject = "SendOTP";
            mail.Body = otpString;

            var result = await _mailHelpers.SendMail(mail);

            return result
                ? APIResults<bool>.Success(true, _localizer[Messages.SendMailSuccess])
                : APIResults<bool>.Failure(_localizer[Messages.SendMailFailure]);
        }

        private string GenerateOtp(string email)
        {
            // 1. Sinh OTP 6 chữ số an toàn
            byte[] rngBytes = new byte[4];
            RandomNumberGenerator.Fill(rngBytes);
            var otp = BitConverter.ToUInt32(rngBytes, 0) % 1000000;
            var otpString = otp.ToString("D6");

            // 2. Tạo salt + hash OTP
            var salt = Guid.CreateVersion7().ToString();
            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(otpString + salt));
            var hashed = Convert.ToBase64String(hashBytes);

            // 3. Lưu vào store tạm
            otpStore[email] = new OtpEntry
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

        public async Task<APIResults<UserAccessModel>> GetMyAccess()
        {
            var userId = _currentUser.UserId;
            if (userId == Guid.Empty)
                return APIResults<UserAccessModel>.Failure(_localizer[Messages.NotFoundGet]);

            var roleNames = await (
                from ur in _context.UserRoles.AsNoTracking()
                join r in _context.Roles.AsNoTracking() on ur.RoleId equals r.Id
                where ur.UserId == userId
                    && ur.IsDelete != true
                    && ur.IsActive != false
                    && r.IsDelete != true
                    && r.IsActive != false
                    && !string.IsNullOrWhiteSpace(r.RoleName)
                select r.RoleName!)
                .Distinct()
                .ToListAsync();

            var permissions = await _permissionService.GetUserPermissions(userId);

            var result = new UserAccessModel
            {
                RoleName = roleNames.Count > 0 ? string.Join(", ", roleNames) : null,
                Permissions = permissions.ToList()
            };

            return APIResults<UserAccessModel>.Success(result, _localizer[Messages.GetResultSuccess]);
        }

        #endregion
    }
}
