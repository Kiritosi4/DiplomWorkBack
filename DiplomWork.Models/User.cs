using DiplomWork.DTO;

namespace DiplomWork.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }

        public ICollection<Budget>? Budgets { get; set; }
        public ICollection<Expense>? Expenses { get; set; }
        public ICollection<Target>? Targets { get; set; }
        public ICollection<Profit>? Profits { get; set; }
        public ICollection<ExpenseCategory>? ExpenseCategories { get; set; }
        public ICollection<ProfitCategory>? ProfitCategories { get; set; }

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
