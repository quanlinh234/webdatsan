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
using webdatsan.Controllers;

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
    //    private ClaimsIdentity GenerateClaims(Users user)
    //    {
    //        var claims = new List<Claim>
    //{
    //    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString() ?? string.Empty),
    //    new Claim(ClaimTypes.Name, user.FullName ?? string.Empty),
    //    new Claim(ClaimTypes.Name, user.Username ?? string.Empty),  
    //    // Tên đầy đủ
    //    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),           // Email
    //    new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty), // Số điện thoại
    //    new Claim("FCMToken", user.FCMToken ?? string.Empty),              // FCMToken, sử dụng tên tùy chỉnh
    //    new Claim("DateOfBirth", user.DateOfBirth?.ToString("yyyy-MM-dd") ?? string.Empty), // Ngày sinh
    //    new Claim("Gender", user.Gender?.ToString() ?? string.Empty),      // Giới tính
    //    new Claim(ClaimTypes.StreetAddress, user.Address ?? string.Empty), // Địa chỉ
    //    new Claim(ClaimTypes.Role, user.Role.ToString())                   // Vai trò
    //};

    //        return new ClaimsIdentity(claims);
    //    }

        
        //public string GenerateToken(Users user)
        //{
        //    var handler = new JwtSecurityTokenHandler();
        //    var key = Encoding.ASCII.GetBytes("0123456789012345678901234567890123456789");
        //    var credentials = new SigningCredentials(
        //        new SymmetricSecurityKey(key),
        //        SecurityAlgorithms.HmacSha1Signature);

        //    var tokenDescriptor = new SecurityTokenDescriptor
        //    {
        //        Subject = GenerateClaims(user),
        //        Expires = DateTime.UtcNow.AddDays(1),

        //        SigningCredentials = credentials,
        //    };

        //    var token = handler.CreateToken(tokenDescriptor);
        //    return handler.WriteToken(token);
        //}

        //
        
        //public Users ValidateToken(string token)
        //{

        //    using (MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("ketnoi")))
        //    {
        //        con.Open();
        //        string querycheck = "SELECT COUNT(*) FROM users WHERE Token = @Token";

        //        using (MySqlCommand cmd = new MySqlCommand(querycheck, con))
        //        {
        //            cmd.Parameters.AddWithValue("@Token", token);

        //            int count = Convert.ToInt32(cmd.ExecuteScalar());
        //            if (count > 0)
        //            {
        //                var tokenHandler = new JwtSecurityTokenHandler();
        //                var key = Encoding.ASCII.GetBytes("0123456789012345678901234567890123456789");

        //                var validationParameters = new TokenValidationParameters
        //                {
        //                    ValidateIssuerSigningKey = true,
        //                    IssuerSigningKey = new SymmetricSecurityKey(key),
        //                    ValidateIssuer = false,
        //                    ValidateAudience = false,
        //                    ClockSkew = TimeSpan.Zero
        //                };

        //                try
        //                {
        //                    ClaimsPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

        //                    if (validatedToken is JwtSecurityToken jwtToken &&
        //                        jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha1, StringComparison.InvariantCultureIgnoreCase))
        //                    {
        //                        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //                        var userName = principal.FindFirst(ClaimTypes.Name)?.Value;
        //                        var email = principal.FindFirst(ClaimTypes.Email)?.Value;
        //                        var roleString = principal.FindFirst(ClaimTypes.Role)?.Value;
        //                        // Lấy thêm các claim khác nếu có
        //                        var fullName = principal.FindFirst("FullName")?.Value;
        //                        var phoneNumber = principal.FindFirst("PhoneNumber")?.Value;
        //                        var address = principal.FindFirst("Address")?.Value;
        //                        var dateOfBirth = principal.FindFirst("DateOfBirth")?.Value;
        //                        var gender = principal.FindFirst("Gender")?.Value;

        //                        int id = userId != null ? int.Parse(userId) : 0;
        //                        short role = roleString != null ? short.Parse(roleString) : (short)0;

        //                        Users user = new Users
        //                        {
        //                            Id = id,
        //                            Username = userName,
        //                            Email = email,
        //                            Role = role,
        //                            FullName = fullName,
        //                            PhoneNumber = phoneNumber,
        //                            Address = address,
        //                            DateOfBirth = dateOfBirth != null ? DateTime.Parse(dateOfBirth) : (DateTime?)null,
        //                            Gender = gender != null ? short.Parse(gender) : (short?)null,
        //                        };

        //                        // Trả về đối tượng user
        //                        return user;

        //                    }
        //                }
        //                catch
        //                {
        //                    // Nếu token không hợp lệ, trả về null
        //                    return null;
        //                }

        //            }
        //            else
        //            {
        //                // Token không tồn tại trong database, tức là không hợp lệ
        //                return null;
        //            }
        //        }
        //    }
        //    return null;


        //}




        [HttpPost]
        [Route("checktokenhople")]
        public IActionResult CheckToken(string token)
        {
            TK tken = new TK();
            var principal = tken.ValidateToken(token);
            if (principal == null)
            {
                return Unauthorized("Token không hợp lệ hoặc đã hết hạn.");

            }
            return Ok("Thành Công ");
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
                TK tken = new TK();
                string token = tken.GenerateToken(user);


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
                            // Kiểm tra nếu trường "Email" là NULL trước khi đọc
                            Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString("Email"),
                            // Kiểm tra nếu trường "PhoneNumber" là NULL trước khi đọc
                            PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString("PhoneNumber"),
                            // Kiểm tra nếu trường "FullName" là NULL trước khi đọc
                            FullName = reader.IsDBNull(reader.GetOrdinal("Fullname")) ? null : reader.GetString("Fullname"),
                            // Kiểm tra nếu trường "DateOfBirth" là NULL trước khi đọc
                            DateOfBirth = reader.IsDBNull(reader.GetOrdinal("DateOfBirth")) ? (DateTime?)null : reader.GetDateTime("DateOfBirth"),
                            // Kiểm tra nếu trường "Gender" là NULL trước khi đọc
                            Gender = reader.IsDBNull(reader.GetOrdinal("Gender")) ? (byte?)null : reader.GetByte("Gender"),
                            // Kiểm tra nếu trường "Address" là NULL trước khi đọc
                            Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString("Address"),
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
                            Id = reader.GetInt32("Id"), // Giả sử trường này không thể NULL
                                                        // Kiểm tra xem Email có phải NULL không
                            Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString("Email"),
                            // Kiểm tra xem PhoneNumber có phải NULL không
                            PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString("PhoneNumber"),
                            // Kiểm tra xem FullName có phải NULL không
                            FullName = reader.IsDBNull(reader.GetOrdinal("Fullname")) ? null : reader.GetString("Fullname"),
                            // Kiểm tra xem DateOfBirth có phải NULL không
                            DateOfBirth = reader.IsDBNull(reader.GetOrdinal("DateOfBirth")) ? (DateTime?)null : reader.GetDateTime("DateOfBirth"),
                            // Kiểm tra xem Gender có phải NULL không
                            Gender = reader.IsDBNull(reader.GetOrdinal("Gender")) ? (byte?)null : reader.GetByte("Gender"),
                            // Kiểm tra xem Address có phải NULL không
                            Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString("Address"),
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
            if (user.Token == null)
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
                        TK tken = new TK();
                        tken.GenerateToken(user);
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
                        cmd.Parameters.AddWithValue("@Token", user.Token);
                        var tokenFromDb = cmd.ExecuteScalar()?.ToString();

                        if (tokenFromDb == null)
                        {
                            con.Close();
                            return BadRequest("Người dùng không tồn tại.");
                        }

                        TK tken = new TK();
                        Users user1 = tken.ValidateToken(tokenFromDb);
                        Console.WriteLine(user1);

                        if (user1 != null)
                        {
                            
                            return Ok(user1) ;
                        }
                        else
                        {
                            return Unauthorized("Token không hợp lệ hoặc đã hết hạn.");
                        }

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

            using (MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("ketnoi")))
            {
                con.Open();

                string query = "UPDATE users SET Username = @Username, PhoneNumber = @PhoneNumber, FullName = @FullName, DateOfBirth = @DateOfBirth, Gender = @Gender, Address = @Address WHERE Email = @Email";

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@username", user.Username);
                    cmd.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
                    cmd.Parameters.AddWithValue("@FullName", user.FullName);
                    cmd.Parameters.AddWithValue("@DateOfBirth", user.DateOfBirth);
                    cmd.Parameters.AddWithValue("@Gender", user.Gender);
                    cmd.Parameters.AddWithValue("@Address", user.Address);
                    cmd.Parameters.AddWithValue("@Email", user.Email);


                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Ok("Thông tin người dùng đã được cập nhật thành công.");
                    }
                    else
                    {
                        return StatusCode(500, "Lỗi khi cập nhật thông tin người dùng.");
                    }
                }
            }
        }
        //Chức năng thay đổi ít thông tin 
        [HttpPost]
        [Route("ThaydoiTT")]
        public IActionResult ThaydoiTT([FromBody] Users user)
        {
            if (user == null || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.HashedPassword))
            {
                return BadRequest("Thông tin người dùng không hợp lệ.");
            }

            using (MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("ketnoi")))
            {
                con.Open();

                string query = "UPDATE users SET PhoneNumber = @PhoneNumber, FullName = @FullName, DateOfBirth = @DateOfBirth, Gender = @Gender, Address = @Address ,Email = @Email WHERE Username = @Username";

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@username", user.Username);
                    cmd.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
                    cmd.Parameters.AddWithValue("@FullName", user.FullName);
                    cmd.Parameters.AddWithValue("@DateOfBirth", user.DateOfBirth);
                    cmd.Parameters.AddWithValue("@Gender", user.Gender);
                    cmd.Parameters.AddWithValue("@Address", user.Address);
                    cmd.Parameters.AddWithValue("@Email", user.Email);


                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Ok("Thông tin người dùng đã được cập nhật thành công.");
                    }
                    else
                    {
                        return StatusCode(500, "Lỗi khi cập nhật thông tin người dùng.");
                    }
                }
            }
        }

        // Thay đổi thông tin bằng token
        [HttpPost]
        [Route("ThaydoiTTbangTK")]
        public IActionResult ThaydoiTT([FromQuery] string token, [FromBody] Users user)
        {

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token không hợp lệ.");
            }

            // Xác thực token
            TK tken = new TK();
            Users authenticatedUser = tken.ValidateToken(token);
            if (authenticatedUser == null)
            {
                return Unauthorized("Token không hợp lệ.");
            }

            // Kiểm tra nếu Username và Email trong token không khớp với thông tin từ body
            if (authenticatedUser.Username != user.Username || authenticatedUser.Email != user.Email)
            {
                return Unauthorized("Username hoặc Email không hợp lệ.");
            }


            // Kết nối cơ sở dữ liệu và cập nhật thông tin người dùng
            using (MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("ketnoi")))
            {
                con.Open();

                string query = "UPDATE users SET PhoneNumber = @PhoneNumber, FullName = @FullName, DateOfBirth = @DateOfBirth, Gender = @Gender, Address = @Address, Email = @Email WHERE Username = @Username";

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Username", user.Username);
                    cmd.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
                    cmd.Parameters.AddWithValue("@FullName", user.FullName);
                    cmd.Parameters.AddWithValue("@DateOfBirth", user.DateOfBirth);
                    cmd.Parameters.AddWithValue("@Gender", user.Gender);
                    cmd.Parameters.AddWithValue("@Address", user.Address);
                    cmd.Parameters.AddWithValue("@Email", user.Email);

                    int rowsAffected = cmd.ExecuteNonQuery();


                    if (rowsAffected > 0)
                    {
                        // Kiểm tra xem có cần tạo lại token không
                        bool isSensitiveDataChanged = authenticatedUser.Email != user.Email || authenticatedUser.Username != user.Username;
                        if (isSensitiveDataChanged)
                        {
                            // Tạo lại token mới với thông tin đã cập nhật
                            
                            string newToken = tken.GenerateToken(user); // Hàm tạo token mới
                            return Ok(new { Message = "Thông tin người dùng đã được cập nhật thành công.", Token = newToken });
                        }

                        return Ok("Thông tin người dùng đã được cập nhật thành công.");
                    }
                    else
                    {
                        return StatusCode(500, "Lỗi khi cập nhật thông tin người dùng.");
                    }
                }
            }
        }

        //Chức năng thay đổi mật khẩu 
        [HttpPost]
        [Route("DoiMK")]
        public IActionResult DoiMK([FromBody] Users user)
        {
            if (user == null || string.IsNullOrEmpty(user.Email) && string.IsNullOrEmpty(user.HashedPassword))
            {
                return BadRequest("Thông tin người dùng không hợp lệ.");
            }
            user.HashedPassword = _passwordHasher.HashPassword(user, user.HashedPassword);

            using (MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("ketnoi")))
            {
                con.Open();

                string query = "UPDATE users SET HashedPassword = @HashedPassword WHERE Email = @Email";

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@HashedPassword", user.HashedPassword);
                    cmd.Parameters.AddWithValue("@Email", user.Email);


                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Ok("Thông tin người dùng đã được cập nhật thành công.");
                    }
                    else
                    {
                        return StatusCode(500, "Lỗi khi cập nhật mật khẩu người dùng.");
                    }
                }
            }
        }
        //Chức năng thay đổi khẩu bằng token
        [HttpPost]
        [Route("DoiMKbangTK")]
        public IActionResult DoiMK([FromQuery] string token, [FromBody] string newPassword)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token không hợp lệ.");
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                return BadRequest("Mật khẩu mới không hợp lệ.");
            }
            TK tken = new TK();

            // Xác thực token và lấy thông tin người dùng
            Users user = tken.ValidateToken(token);
            if (user == null)
            {
                return Unauthorized("Token không hợp lệ hoặc đã hết hạn.");
            }

            // Hash mật khẩu mới
            user.HashedPassword = _passwordHasher.HashPassword(user, newPassword);

            // Kết nối cơ sở dữ liệu và cập nhật mật khẩu
            using (MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("ketnoi")))
            {
                con.Open();

                string query = "UPDATE users SET HashedPassword = @HashedPassword WHERE Email = @Email";

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@HashedPassword", user.HashedPassword);
                    cmd.Parameters.AddWithValue("@Email", user.Email);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Ok("Mật khẩu đã được cập nhật thành công.");
                    }
                    else
                    {
                        return StatusCode(500, "Lỗi khi cập nhật mật khẩu người dùng.");
                    }
                }
            }
        }

        //Chức năng reset mật khẩu 
        [HttpPost]
        [Route("resetMK")]
        public IActionResult ResetMK([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token không hợp lệ.");
            }
            TK tken = new TK();
            // Xác thực token và lấy thông tin người dùng
            Users user = tken.ValidateToken(token);
            if (user == null)
            {
                return Unauthorized("Token không hợp lệ hoặc đã hết hạn.");
            }

            // Tạo mật khẩu tạm thời và hash nó
            var tempPassword = Guid.NewGuid().ToString().Substring(0, 8);
            var hashedPassword = _passwordHasher.HashPassword(user, tempPassword);

            // Kết nối cơ sở dữ liệu và cập nhật mật khẩu
            using (MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("ketnoi")))
            {
                con.Open();

                string query = "UPDATE users SET HashedPassword = @HashedPassword WHERE Email = @Email";

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@HashedPassword", hashedPassword);
                    cmd.Parameters.AddWithValue("@Email", user.Email);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Ok("Mật khẩu đã được đặt lại thành công. Mật khẩu mới là: " + tempPassword);
                    }
                    else
                    {
                        return StatusCode(500, "Lỗi khi đặt lại mật khẩu.");
                    }
                }
            }
        }


        //
        [HttpGet]
        [Route("PageUser")]
        public IActionResult PageUser([FromQuery] int page = 0, [FromQuery] int size = 10)
        {
            List<object> users = new List<object>();
            int totalUsers = 0;

            using (MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("ketnoi")))
            {
                con.Open();

                // Truy vấn tổng số người dùng
                string countQuery = "SELECT COUNT(*) FROM users";
                using (MySqlCommand countCmd = new MySqlCommand(countQuery, con))
                {
                    totalUsers = Convert.ToInt32(countCmd.ExecuteScalar());
                }

                // Tính tổng số trang dựa trên số lượng người dùng và kích thước trang
                int totalPages = (int)Math.Ceiling((double)totalUsers / size);

                // Truy vấn dữ liệu người dùng với phân trang
                string query = "SELECT * FROM users LIMIT @Offset, @Size";
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Offset", page * size);
                    cmd.Parameters.AddWithValue("@Size", size);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new
                            {
                                Id = reader["Id"],
                                Email = reader["Email"],
                                PhoneNumber = reader["PhoneNumber"],
                                FullName = reader["FullName"],
                                DateOfBirth = reader["DateOfBirth"],
                                Gender = reader["Gender"],
                                Address = reader["Address"],
                                Role = reader["Role"]
                            });
                        }
                    }
                }

                // Kiểm tra nếu không có dữ liệu
                if (users.Count == 0)
                {
                    return BadRequest("Không có dữ liệu");
                }

                // Tạo response
                var response = new
                {
                    totalPages = totalPages,
                    currentPage = page,
                    users = users
                };

                return Ok(response);
            }
        }
        // đăng ký bằng google
        [HttpPost]
        [Route("GoogleLogin")]
        public async Task<IActionResult> GoogleLogin([FromBody] string idToken)
        {
            try
            {
                // Xác minh token với Google
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);

                if (payload == null)
                {
                    return BadRequest("Token không hợp lệ.");
                }

                var email = payload.Email;
                var fullName = payload.Name;

                // Tạo mật khẩu ngẫu nhiên cho người dùng mới
                var tempPassword = Guid.NewGuid().ToString().Substring(0, 8);
                var hashedPassword = _passwordHasher.HashPassword(null, tempPassword);

                // Kết nối cơ sở dữ liệu MySQL
                using (MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("ketnoi")))
                {
                    con.Open();

                    // Kiểm tra xem email đã tồn tại chưa
                    string queryCheck = "SELECT COUNT(*) FROM users WHERE Email = @Email";
                    using (MySqlCommand cmdCheck = new MySqlCommand(queryCheck, con))
                    {
                        cmdCheck.Parameters.AddWithValue("@Email", email);
                        int count = Convert.ToInt32(cmdCheck.ExecuteScalar());

                        if (count > 0)
                        {
                            return BadRequest("Email đã tồn tại");
                        }
                    }

                    // Thêm người dùng mới vào cơ sở dữ liệu
                    string queryInsert = "INSERT INTO users (Username, Email, HashedPassword, Role) VALUES (@Email, @Email, @HashedPassword, 0)";
                    using (MySqlCommand cmdInsert = new MySqlCommand(queryInsert, con))
                    {
                        cmdInsert.Parameters.AddWithValue("@Username", email);

                        cmdInsert.Parameters.AddWithValue("@HashedPassword", hashedPassword);

                        cmdInsert.ExecuteNonQuery();
                    }

                    con.Close();
                }

                // Trả về thông tin người dùng
                // LƯU Ý SAU KHI DK THÀNH CÔNG CÓ TRẢ VỀ MK RANDOM , EMAIL CHO NGƯỜI DÙNG THÌ FE CHUYEN QUA TRANG ĐỔI MK CHO NGƯỜI DÙNG ĐỔI LẠI MK MỚI
                return Ok(new
                {
                    Email = email,
                    FullName = fullName,
                    tempPassword = tempPassword,
                    Message = "Đăng ký thành công"
                });
            }
            catch
            {
                return BadRequest("Đã xảy ra lỗi khi xác thực với Google.");
            }
        }



        //


        [HttpPost]
        [Route("QuenMK-XN-email")]
        public async Task<IActionResult> SendEmailAsync([FromBody] Users user)
        {
            string subject;
            string body;
            string toEmail = user.Email;
            string token = null;
            if (user == null || user.Email == null)
            {
                return BadRequest("Thông tin không hợp lệ");
            }
            using (MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("ketnoi")))
            {
                con.Open();
                string query = "SELECT Email , Token FROM users WHERE Email = @Email ";


                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Lấy token từ kết quả truy vấn
                            token = reader["Token"].ToString();

                            var EmailCheck = cmd.ExecuteScalar()?.ToString();
                            if (EmailCheck == null)
                            {
                                con.Close();
                                return BadRequest("Không tồn tại Email");
                            }

                        }
                    }
                }
                body = $"Click vào đây để reset mật khẩu của bạn: <a href=' http://localhost:3000/resetMK?token={token}'>reset password</a>";

                subject = " THÔNG BÁO XÁC NHẬN ĐẶT LẠI MẬT KHẨU CỦA WEB ĐẶT SÂN THỂ THAO ";

                var email = new MimeMessage();

                email.From.Add(new MailboxAddress(_configuration["EmailSettings:SenderName"], _configuration["EmailSettings:SenderEmail"]));

                email.To.Add(new MailboxAddress(toEmail, toEmail));

                email.Subject = subject;

                var builder = new BodyBuilder { HtmlBody = body };
                email.Body = builder.ToMessageBody();

                using (var smtp = new MailKit.Net.Smtp.SmtpClient())
                {
                    smtp.Connect(_configuration["EmailSettings:SmtpServer"], int.Parse(_configuration["EmailSettings:SmtpPort"]), MailKit.Security.SecureSocketOptions.StartTls);

                    smtp.Authenticate(_configuration["EmailSettings:SenderEmail"], _configuration["EmailSettings:SenderPassword"]);

                    await smtp.SendAsync(email);

                    smtp.Disconnect(true);
                }
                return Ok();
            }
        }






    }

}


