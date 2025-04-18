

namespace DiplomWork.DTO
{
    public record AddBudgetDTO
    {
        public string Name { get; set; }
        public Guid? Category { get; set; }
        public decimal Limit { get; set; }
        public int? PeriodType { get; set; }
        public int TimezoneOffset { get; set; } = 0;

        public long StartPeriod { get; set; } = 0;
        public long EndPeriod { get; set; } = 0;
    }
}
