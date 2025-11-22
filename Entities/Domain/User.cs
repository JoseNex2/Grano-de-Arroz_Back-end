namespace Entities.Domain
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string NationalId { get; set; }
        public string PhoneNumber {  get; set; }
        public string Password { get; set; }
        public DateTime RegisteredDate { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; }
        public ICollection<SecureRandomToken> SecureRandomTokens { get; set; } = new List<SecureRandomToken>();
    }
}
