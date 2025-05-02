using DiplomWork.DTO;

namespace DiplomWork.Models
{
    public class Budget
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Limit { get; set; }
        public Guid? CategoryId { get; set; }
        public ExpenseCategory? Category { get; set; }
        public Guid OwnerId { get; set; }
        public Period PeriodType { get; set; }
        public long StartPeriod { get; set; }
        public long EndPeriod { get; set; }
        public List<Expense>? Expenses { get; set; }


        public BudgetDTO ConvertToDTO(int timezoneOffset = 0)
        {
            var minTimestamp = StartPeriod;
            var maxTimestamp = EndPeriod;
            if (PeriodType != Period.Custom)
            {
                minTimestamp = GetStartOfPeriod(PeriodType, timezoneOffset);
                maxTimestamp = GetEndOfPeriod(PeriodType, timezoneOffset);
            }

            decimal amount = 0M;
            if (Expenses != null)
            {
                try
                {
                    amount = Expenses.Where(x => x.CreatedAt >= minTimestamp && x.CreatedAt < maxTimestamp).Sum(x => x.Amount);
                }
                catch (OverflowException)
                {
                    amount = decimal.MaxValue;
                }
            }

            return new BudgetDTO
            {
                Id = Id,
                Name = Name,
                Limit = Limit,
                Amount = amount,
                PeriodType = (short)PeriodType,
                StartPeriod = minTimestamp,
                EndPeriod = maxTimestamp,
                CategoryId = CategoryId
            };
        }

        public static long GetStartOfPeriod(Period period, int timezoneOffset = 0)
        {
            var now = DateTime.UtcNow.AddHours(timezoneOffset);
            long result = 0;

            switch (period)
            {
                case Period.Week:
                    int daysToSubtract = (now.DayOfWeek - DayOfWeek.Monday + 7) % 7;
                    result = new DateTimeOffset(now.Date.AddDays(-daysToSubtract)).ToUnixTimeSeconds();
                    break;

                case Period.Month:
                    result = new DateTimeOffset(new DateTime(now.Year, now.Month, 1)).ToUnixTimeSeconds();
                    break;

                case Period.Year:
                    result = new DateTimeOffset(new DateTime(now.Year, 1, 1)).ToUnixTimeSeconds();
                    break;

                case Period.Day:
                    result = new DateTimeOffset(now.Date).ToUnixTimeSeconds();
                    break;
            }

            return result - timezoneOffset * 3600;
        }

        public static long GetEndOfPeriod(Period period, int timezoneOffset = 0)
        {
            DateTime now = DateTime.UtcNow.AddHours(timezoneOffset);
            long result = 0;

            switch (period)
            {
                case Period.Day:
                    result = new DateTimeOffset(now.Date.AddDays(1)).ToUnixTimeSeconds();
                    break;

                case Period.Week:
                    int daysToSubtract = (now.DayOfWeek - DayOfWeek.Monday + 7) % 7;
                    DateTime currentMonday = now.Date.AddDays(-daysToSubtract);
                    result = new DateTimeOffset(currentMonday.AddDays(7)).ToUnixTimeSeconds();
                    break;

                case Period.Month:
                    result = new DateTimeOffset(new DateTime(now.Year, now.Month, 1).AddMonths(1)).ToUnixTimeSeconds();
                    break;

                case Period.Year:
                    result = new DateTimeOffset(new DateTime(now.Year + 1, 1, 1)).ToUnixTimeSeconds();
                    break;
            }

            return result - timezoneOffset * 3600;
        }

    }
}
