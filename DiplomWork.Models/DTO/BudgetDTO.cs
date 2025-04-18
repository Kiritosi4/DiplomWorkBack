

using DiplomWork.Models;

namespace DiplomWork.DTO
{
    public record BudgetDTO
    {
        public Guid Id { get; set; }
        public Guid? CategoryId { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public decimal Limit { get; set; }
        public short PeriodType { get; set; }
        public long StartPeriod { get; set; }
        public long EndPeriod { get; set; }
    }

    public class BudgetListDTO : EntityListDTO<BudgetDTO>
    {
        public ExpenseCategory[] Categories { get; set; }
    }
}
