using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Google.Apis.Auth;
using webdatsan.Models;

namespace webdatsan.Controllers
{
    [Route("api/")]
    [ApiController]
    public class DkDnController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        
        private readonly PasswordHasher<Users> _passwordHasher = new PasswordHasher<Users>();

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

       

        public DkDnController(IConfiguration configuration)
        {
            _configuration = configuration;

        }
        //
        private ClaimsIdentity GenerateClaims(Users user)
{
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString() ?? string.Empty),
        new Claim(ClaimTypes.Name, user.FullName ?? string.Empty),
        new Claim(ClaimTypes.Name, user.Username ?? string.Empty),  
        // Tên đầy đủ
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
        var key = Encoding.ASCII.GetBytes("1qaz2wsx");
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

        //
        
        public Users  ValidateToken(string token)
{
    
using (MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("ketnoi")))
{
    con.Open();
    string querycheck = "SELECT COUNT(*) FROM users WHERE Token = @Token";

    using (MySqlCommand cmd = new MySqlCommand(querycheck, con))
    {
        cmd.Parameters.AddWithValue("@Token", token);

        int count = Convert.ToInt32(cmd.ExecuteScalar());
        if (count > 0)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes("1qaz2wsx");

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
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = principal.FindFirst(ClaimTypes.Name)?.Value;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            var roleString = principal.FindFirst(ClaimTypes.Role)?.Value;
            // Lấy thêm các claim khác nếu có
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
        Role = role  ,
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
            
        }
        else
        {
            // Token không tồn tại trong database, tức là không hợp lệ
            return null;
        }
    }
    }
return null;


}


         
    
        [HttpPost]
        [Route("checktokenhople")]
            public IActionResult CheckToken(string token) {
            var principal = ValidateToken(token);
            if (principal == null)
            {
                return Unauthorized("Token không hợp lệ hoặc đã hết hạn.");
                
            }
            return Ok("Thành Công ") ;
        }


// Dang ky
        [HttpPost]
        [Route("Dangky")]
        public IActionResult Dangky([FromBody] Users user)
        {

            if (user == null || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.HashedPassword))
            {
                return BadRequest("Thông tin người dùng không hợp lệ.");
            }

            if (!IsValidEmail(user.Email))
            {
                return BadRequest("Địa chỉ email không hợp lệ.");
            }

            


            using (MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("ketnoi")))
            {
                con.Open();
                string querycheck = "SELECT COUNT(*) FROM users WHERE Email = @Email";

                using (MySqlCommand cmd = new MySqlCommand(querycheck, con))
                {
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    if (count > 0)
                    {
                        con.Close();
                        return BadRequest("Email đã tồn tại");
                    }

                }
                user.HashedPassword = _passwordHasher.HashPassword(user, user.HashedPassword);

            string token = GenerateToken(user);


                string query = "INSERT INTO users (Username ,Email, HashedPassword, Role ,Token) VALUES (@Email ,@Email, @HashedPassword, 0 ,@Token)";

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@HashedPassword", user.HashedPassword);
                    cmd.Parameters.AddWithValue("@Token", token);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        con.Close();
                        return Ok("Người dùng đã được đăng ký thành công.");
                    }
                    else
                    {
                        con.Close();
                        return StatusCode(500, "Lỗi khi đăng ký người dùng.");
                    }
                }
            }
        }
        
        //chuc nang lay thong tin nhieu nguoi dung data

        [HttpGet]
        [Route("info-users")]
        public IActionResult infoUsers()
        {
            using (MySqlConnection con2 = new MySqlConnection(_configuration.GetConnectionString("ketnoi")))
            {
                con2.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM users", con2);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    var usersList = new List<Users>();

                    while (reader.Read())
                    {
                        var userData = new Users
                        {
                            Id = reader.GetInt32("Id"),
                            Email = reader.GetString("Email"),
                            PhoneNumber = reader.GetString(" PhoneNumber"),

                            FullName = reader.GetString("Fullname"),
                            DateOfBirth = reader.GetDateTime("DateOfBirth"),
                            Gender = reader.GetByte("Gender"),
                            Address = reader.GetString("Address"),
                        
                        };

                        usersList.Add(userData);
                    }

                    return Ok(usersList);
                }
            }
        }
//Chức năng lấy thông tin 1 người dùng /info-users?email=user@gmail.com
        [HttpGet]
[Route("email-info-users")]
public IActionResult infoUsers([FromQuery] string email)
{
    using (MySqlConnection con2 = new MySqlConnection(_configuration.GetConnectionString("ketnoi")))
    {
        con2.Open();
        MySqlCommand cmd = new MySqlCommand("SELECT * FROM users WHERE Email = @Email", con2);
        cmd.Parameters.AddWithValue("@Email", email);

        using (MySqlDataReader reader = cmd.ExecuteReader())
        {
            var usersList = new List<Users>();

            while (reader.Read())
            {
                var userData = new Users
                {
                    Id = reader.GetInt32("Id"),
                    Email = reader.GetString("Email"),
                    PhoneNumber = reader.GetString("PhoneNumber"),
                    FullName = reader.GetString("Fullname"),
                    DateOfBirth = reader.GetDateTime("DateOfBirth"),
                    Gender = reader.GetByte("Gender"),
                    Address = reader.GetString("Address"),
                };

                usersList.Add(userData);
            }

            return Ok(usersList);
        }
    }
}

        //
        
        [HttpPost]
        [Route("DNhap")]
        public IActionResult DNhap([FromBody] Users user)
        {
            if(user.Token == null)
            {
            if (user == null || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.HashedPassword))
            {
                return BadRequest("Thông tin người dùng không hợp lệ.");
            }
            using (MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("ketnoi")))
            {
                con.Open();
                string query = "SELECT HashedPassword  FROM users WHERE Email = @Email";
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@HashedPassword", user.HashedPassword);

                    var hashedPasswordFromDb = cmd.ExecuteScalar()?.ToString();

                    if (hashedPasswordFromDb == null)
                    {
                        con.Close();
                        return BadRequest("Người dùng không tồn tại.");
                    }

                    var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(new Users(), hashedPasswordFromDb, user.HashedPassword);
GenerateToken(user);
                    if (passwordVerificationResult == PasswordVerificationResult.Success)
                    {
                        con.Close();
                        return Ok("Đăng nhập thành công.");
                    }
                    else
                    {
                        con.Close();
                        return Unauthorized("Mật khẩu không đúng.");
                    }
                }
            }
            return Ok();
                }
            else
            {
                
    using (MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("ketnoi")))
            {
                con.Open();
                string query = "SELECT Token  FROM users WHERE Token = @Token";
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Token", user.Token );
             var tokenFromDb = cmd.ExecuteScalar()?.ToString();

                    if (tokenFromDb == null)
                    {
                        con.Close();
                        return BadRequest("Người dùng không tồn tại.");
                        }
                    ValidateToken(tokenFromDb);

                }
            }
                return Ok("ok");
        }
            
}
// Chức năng thay đổi nhiều thông tin 
        
            
            [HttpPost]
        [Route("BosungTT")]
        public IActionResult BosungTT([FromBody] Users user)
        {
            if (user == null || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.HashedPassword))
            {
                return BadRequest("Thông tin người dùng không hợp lệ.");
            }