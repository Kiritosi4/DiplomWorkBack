

namespace DiplomWork.DTO
{
    public class CreateExpenseDTO
    {
        public Guid? CategoryId { get; set; }
        public decimal Amount { get; set; }
        public long Timestamp { get; set; }
        public Guid? BudgetId { get; set; }
    }
}
