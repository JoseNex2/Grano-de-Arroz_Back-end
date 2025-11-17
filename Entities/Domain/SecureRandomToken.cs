namespace Entities.Domain
{
    public class SecureRandomToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public DateTime ExpiredDate { get; set; }
        public bool Used { get; set; } = false;
        public DateTime CreatedDate { get; set; }
    }
}
