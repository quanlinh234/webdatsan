namespace webdatsan.Models
{
    public class Users
    {
        public int Id { get; set; }

        public string Username { get; set; }
        public string HashedPassword { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FCMToken { get; set; }
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public short? Gender { get; set; }
        public string? Address { get; set; }
        public short Role { get; set; }
    }
}
