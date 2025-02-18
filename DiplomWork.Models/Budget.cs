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
            var minTimestamp = GetStartOfPeriod(PeriodType);
            var maxTimestamp = GetEndOfPeriod(PeriodType);
            return new BudgetDTO
            {
                Id = Id,
                Name = Name,
                Limit = Limit,
                Amount = Expenses == null ? 0 : Expenses.Where(x => x.CreatedAt > minTimestamp && x.CreatedAt < maxTimestamp).Sum(x => x.Amount),
                PeriodType = (short)PeriodType,
            };
        }

        public static long GetStartOfPeriod(Period period)
        {
            var now = DateTime.Now;

            switch (period)
            {
                case Period.Week:
                    var dayOfWeek = (int)DateTimeOffset.UtcNow.DayOfWeek;
                    dayOfWeek = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
                    long offset = 86400 * dayOfWeek;
                    return new DateTimeOffset(now).ToUnixTimeSeconds() - offset;
                case Period.Month:
                    return new DateTimeOffset(new DateTime(now.Year, now.Month, 1)).ToUnixTimeSeconds();
                case Period.Year:
                    return new DateTimeOffset(new DateTime(now.Year, 1, 1)).ToUnixTimeSeconds();
            }

            return 0;
        }

        public static long GetEndOfPeriod(Period period)
        {
            DateTime now = DateTime.Now;

            switch (period)
            {
                case Period.Day:
                    return new DateTimeOffset(now.Date.AddDays(1)).ToUnixTimeSeconds();

                case Period.Week:
                    int daysUntilNextMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
                    return new DateTimeOffset(now.Date.AddDays(daysUntilNextMonday).AddHours(0)).ToUnixTimeSeconds();

                case Period.Month:
                    return new DateTimeOffset(new DateTime(now.Year, now.Month, 1).AddMonths(1)).ToUnixTimeSeconds();

                case Period.Year:
                    return new DateTimeOffset(new DateTime(now.Year + 1, 1, 1)).ToUnixTimeSeconds();
            }

            return 0;
        }

    }
}
