

namespace DiplomWork.Models
{
    public class ExpenseCategory
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid? OwnerID { get; set; } 
    }
}
