using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Data.SqlClient;
using webdatsan.Models;

namespace webdatsan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DkDnController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public DkDnController(IConfiguration configuration)
        {
            _configuration = configuration;

        }
        [HttpPost]
        [Route("Dangky")]
        public IActionResult Dangky([FromBody] Users user)
        {
            if (user == null || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.HashedPassword))
            {
                return BadRequest("Thông tin người dùng không hợp lệ.");
            }

            using (MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("ketnoi")))
            {
                con.Open();

                string query = "INSERT INTO users (Username, HashedPassword, Role) VALUES (@Username, @HashedPassword, 0)";

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Username", user.Username);
                    cmd.Parameters.AddWithValue("@HashedPassword", user.HashedPassword);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Ok("Người dùng đã được đăng ký thành công.");
                    }
                    else
                    {
                        return StatusCode(500, "Lỗi khi đăng ký người dùng.");
                    }
                }
            }
        }


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
                            Username = reader.GetString("Username"),
                            HashedPassword =reader.GetString("HashedPassword")
                            // Add other properties as needed
                        };
                        usersList.Add(userData);
                    }

                    // Return the list directly; ASP.NET Core will handle JSON serialization
                    return Ok(usersList);
                }
            }
        }



    }

}
    

