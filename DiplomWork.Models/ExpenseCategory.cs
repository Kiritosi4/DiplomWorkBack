using System.Text.Json.Serialization;

namespace DiplomWork.Models
{
    public class ExpenseCategory
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid? OwnerID { get; set; }
        [JsonIgnore]
        public List<Expense>? Expenses { get; set; }
    }
}
