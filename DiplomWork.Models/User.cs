using DiplomWork.DTO;

namespace DiplomWork.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }

        public UserDTO ConvertToDTO()
        {
            return new UserDTO
            {
                Id = Id,
                Email = Email,
                Name = Name
            };
        }
    }
}
