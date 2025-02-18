using DiplomWork.DTO;
using System.Text.Json.Serialization;

namespace DiplomWork.Models
{
    public class Expense
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public decimal Amount { get; set; }
        public long CreatedAt { get; set; }
        public Guid? CategoryId { get; set; }
        [JsonIgnore]
        public ExpenseCategory? Category { get; set; }
        public Guid? BudgetId { get; set; }
        [JsonIgnore]
        public Budget? Budget { get; set; }
    }

    public class ExpensesListDTO : EntityListDTO<Expense>
    {
        public ExpenseCategory[] Categories { get; set; }
    }
}
