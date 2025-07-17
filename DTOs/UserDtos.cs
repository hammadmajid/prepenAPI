namespace PrepenAPI.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public bool IsSuspended { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class UserSuspendDto
    {
        public bool IsSuspended { get; set; }
    }
}
