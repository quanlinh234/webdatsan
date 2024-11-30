using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using webdatsan.Models;

namespace webdatsan.Controllers
{
    public class TK
    {
        private readonly IConfiguration _configuration;
        
        private ClaimsIdentity GenerateClaims(Users user)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString() ?? string.Empty),
        new Claim("Username", user.Username ?? string.Empty),
        new Claim("FullName", user.FullName ?? string.Empty),
        new Claim("PhoneNumber", user.PhoneNumber ?? string.Empty),
        new Claim("Address", user.Address ?? string.Empty),
        new Claim(ClaimTypes.Email, user.Email ?? string.Empty),           // Email
        new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty), // Số điện thoại
        new Claim("FCMToken", user.FCMToken ?? string.Empty),              // FCMToken, sử dụng tên tùy chỉnh
        new Claim("DateOfBirth", user.DateOfBirth?.ToString("yyyy-MM-dd") ?? string.Empty), // Ngày sinh
        new Claim("Gender", user.Gender?.ToString() ?? string.Empty),      // Giới tính
        new Claim(ClaimTypes.StreetAddress, user.Address ?? string.Empty), // Địa chỉ
        new Claim(ClaimTypes.Role, user.Role.ToString())                   // Vai trò
    };

            return new ClaimsIdentity(claims);
        }
        public string GenerateToken(Users user)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("0123456789012345678901234567890123456789");
            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = GenerateClaims(user),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = credentials,
            };

            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }
        public Users ValidateToken(string token)
        {
            Console.WriteLine(token);
           
                        var tokenHandler = new JwtSecurityTokenHandler();
                        var key = Encoding.ASCII.GetBytes("0123456789012345678901234567890123456789");

                        var validationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(key),
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ClockSkew = TimeSpan.Zero
                        };

            try
            {
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                if (validatedToken is JwtSecurityToken jwtToken &&
                    jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine("99999999999999");
                    var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var userName = principal.FindFirst("Username")?.Value;
                    var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                    var roleString = principal.FindFirst(ClaimTypes.Role)?.Value;
                    
                    var fullName = principal.FindFirst("FullName")?.Value;
                    var phoneNumber = principal.FindFirst("PhoneNumber")?.Value;
                    var address = principal.FindFirst("Address")?.Value;
                    var dateOfBirth = principal.FindFirst("DateOfBirth")?.Value;
                    var gender = principal.FindFirst("Gender")?.Value;

                    int id = userId != null ? int.Parse(userId) : 0;
                    short role = roleString != null ? short.Parse(roleString) : (short)0;

                    Users user = new Users
                    {
                        Id = id,
                        Username = userName,
                        Email = email,
                        Role = role,
                        FullName = fullName,
                        PhoneNumber = phoneNumber,
                        Address = address,
                        DateOfBirth = dateOfBirth != null ? DateTime.Parse(dateOfBirth) : (DateTime?)null,
                        Gender = gender != null ? short.Parse(gender) : (short?)null,
                    };
                    
                    // Trả về đối tượng user
                    return user;

                }
            }
            catch
            {
                // Nếu token không hợp lệ, trả về null
                return null;
            }
            return null;


        }
    }
}
