

namespace DiplomWork.DTO
{
    public record BudgetDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public decimal Limit { get; set; }
    }
}
