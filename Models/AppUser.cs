namespace PrepenAPI.Models
{
    public class AppUser
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; } = new byte[0];
        public byte[] PasswordSalt { get; set; } = new byte[0];
        public bool IsSuspended { get; set; }
        public bool IsDeleted { get; set; }
        public List<Order> Orders { get; set; } = new();
    }
}
