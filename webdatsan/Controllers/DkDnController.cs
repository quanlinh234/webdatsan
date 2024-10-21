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

            user.HashedPassword = _passwordHasher.HashPassword(user, user.HashedPassword);


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


                string query = "INSERT INTO users (Username ,Email, HashedPassword, Role) VALUES (@Email ,@Email, @HashedPassword, 0)";

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@HashedPassword", user.HashedPassword);

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
        
        //chuc nang test lay thong tin nguoi dung data

        [HttpGet]
        [Route("Dangnhap")]
        public IActionResult Dangnhap()
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
                            HashedPassword = reader.GetString("HashedPassword")
                            // Add other properties as needed
                        };

                        usersList.Add(userData);
                    }

                    return Ok(usersList);
                }
            }
        }

        
        [HttpPost]
        [Route("DNhap")]
        public IActionResult DNhap([FromBody] Users user)
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

      

        [HttpPost]
        [Route("QuenMK-XN-email")]
        public async Task<IActionResult> SendEmailAsync([FromBody] Users user)
        {
            string subject;
            string body;
            string toEmail= user.Email;
            if (user == null || user.Email == null)
            {
                 return BadRequest("Thông tin không hợp lệ");
            }
            using (MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("ketnoi")))
            {
                con.Open();
                string query = "SELECT Email FROM users WHERE Email = @Email ";

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    var EmailCheck = cmd.ExecuteScalar()?.ToString();
                    if (EmailCheck == null)
                    {
                        con.Close();
                        return BadRequest("Không tồn tại Email");
                    }
                }
            }
            body = $"Click vào đây để reset mật khẩu của bạn: <a href=''>reset password</a>";
            
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
    

