//using System;
//using System.IdentityModel.Tokens.Jwt;

//namespace webdatsan
//{
//    class Program
//    {
//        static void Main(string[] args)
//        {
//            // Tạo một đối tượng TK
//            var tk = new Controllers.TK();

//            // Tạo một user mẫu
//            var user = new Models.Users
//            {
//                Id = 1,
//                Username = "testuser@@@",
//                Email = "testuser@example.com",
//                Role = 1, // Giả sử 1 là Admin
//                FullName = "Test User",
//                PhoneNumber = "0123456789",
//                Address = "123 Test Street",
//                DateOfBirth = new DateTime(1990, 1, 1),
//                Gender = 1, // Giả sử 1 là Nam
//                FCMToken = "testFCMToken123"
//            };

//            // Tạo token từ user mẫu
//            var token = tk.GenerateToken(user);
//            Console.WriteLine("Generated Token:");
//            Console.WriteLine(token);

//            // Decode token để xem thông tin
//            var handler = new JwtSecurityTokenHandler();
//            var jwtToken = handler.ReadJwtToken(token);

//            // Lấy ngày hết hạn từ payload
//            var expirationUnix = jwtToken.Payload.Exp;
//            if (expirationUnix.HasValue)
//            {
//                var expirationDate = DateTimeOffset.FromUnixTimeSeconds(expirationUnix.Value).UtcDateTime;
//                Console.WriteLine($"\nToken Expiration Date (UTC): {expirationDate}");
//            }
//            else
//            {
//                var expirationDate = DateTimeOffset.FromUnixTimeSeconds(expirationUnix.Value).UtcDateTime;
//                Console.WriteLine($"\nToken Expiration Date (UTC): {expirationDate}");
//            }

//            // Kiểm tra validate token
//            Console.WriteLine("\nValidating Token...");
//            var validatedUser = tk.ValidateToken(token);

//            if (validatedUser != null)
//            {
//                Console.WriteLine("Token is valid. User details:");
//                Console.WriteLine($"ID: {validatedUser.Id}");
//                Console.WriteLine($"Username: {validatedUser.Username}");
//                Console.WriteLine($"Email: {validatedUser.Email}");
//                Console.WriteLine($"Role: {validatedUser.Role}");
//                Console.WriteLine($"FullName: {validatedUser.FullName}");
//                Console.WriteLine($"Phone: {validatedUser.PhoneNumber}");
//                Console.WriteLine($"Address: {validatedUser.Address}");
//                Console.WriteLine($"Date of Birth: {validatedUser.DateOfBirth}");
//                Console.WriteLine($"Gender: {validatedUser.Gender}");
//            }
//            else
//            {
//                Console.WriteLine("Token is invalid.");
//            }
//        }
//    }
//}


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
