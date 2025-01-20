using DiplomWork.DTO;

namespace DiplomWork.Models
{
    public class Budget
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Limit { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid OwnerId { get; set; }
        public Period PeriodType { get; set; }
        public List<Expense>? Expenses { get; set; }

        public BudgetDTO ConvertToDTO()
        {
            var minTimestamp = GetPeriodTimestamp(PeriodType);
            return new BudgetDTO
            {
                Id = Id,
                Name = Name,
                Limit = Limit,
                Amount = Expenses == null ? 0 : Expenses.Where(x => x.CreatedAt > minTimestamp).Sum(x => x.Amount)
            };
        }

        public static long GetPeriodTimestamp(Period period)
        {
            var day = DateTimeOffset.UtcNow.Day;
            var month = DateTimeOffset.UtcNow.Month;
            var year = DateTimeOffset.UtcNow.Year;
            long offset = 0;

            switch (period)
            {
                case Period.Week:
                    var dayOfWeek = (int)DateTimeOffset.UtcNow.DayOfWeek;
                    dayOfWeek = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
                    offset = 86400 * dayOfWeek;
                    break;
                case Period.Month:
                    day = 1;
                    break;
                case Period.Year:
                    day = 1;
                    month = 1;
                    break;
            }

            return new DateTimeOffset(new DateTime(year, month, day)).ToUnixTimeSeconds() - offset;
        }
    }
}
