namespace Entities.Domain.DTO
{
    public class UserViewDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string NationalId { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public DateTime DateRegistered { get; set; }
    }
}
