using DiplomWork.DTO;
using DiplomWork.Models;
using DiplomWork.Persistance;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;

namespace DiplomWork.Application.Services
{
    public class ExpensesService
    {
        readonly DiplomWorkDbContext _db;

        public ExpensesService(DiplomWorkDbContext db)
        {
            _db = db;
        }

        public async Task<Expense> AddNewExpense(CreateExpenseDTO createExpenseDTO, Guid userId)
        {
            var newExpense = new Expense
            {
                Id = Guid.NewGuid(),
                Amount = createExpenseDTO.Amount,
                CreatedAt = createExpenseDTO.Timestamp,
                BudgetId = createExpenseDTO.BudgetId,
                CategoryId = createExpenseDTO.CategoryId,
                OwnerId = userId
            };
            await _db.Expenses.AddAsync(newExpense);
            await _db.SaveChangesAsync();

            return newExpense;
        }

        public async Task DeleteExpenseById(Guid id)
        {
            await _db.Expenses.Where(x => x.Id == id).ExecuteDeleteAsync();
        }

        public async Task EditExpense(Guid id, CreateExpenseDTO editedExpense, Guid userId)
        {
            await _db.Expenses
                .Where(x => x.Id == id && x.OwnerId == userId)
                .ExecuteUpdateAsync(x => x
                .SetProperty(x => x.BudgetId, editedExpense.BudgetId)
                .SetProperty(x => x.Amount, editedExpense.Amount)
                .SetProperty(x => x.CreatedAt, editedExpense.Timestamp)
                .SetProperty(x => x.CategoryId, editedExpense.CategoryId));
        }

        public async Task<ExpensesListDTO> GetUserExpenses(Guid userId, int offset = 0, int limit = 10, string orderBy = "CreatedAt", string order = "desc", long? minTimestamp = 0, long? maxTimestamp = 0, Guid?[] categories = null, int timeZoneOffset = 0)
        {
            var query = _db.Expenses
                .AsNoTracking()
                .Where(x => x.OwnerId == userId);

            if (categories != null && categories.Length > 0)
            {
                if (categories.Any(x => x.Value == Guid.Empty))
                {
                    query = query.Where(x => categories.Contains(x.CategoryId) || x.CategoryId == null);
                }
                else
                {
                    query = query.Where(x => categories.Contains(x.CategoryId));
                }
            }

            if (minTimestamp != 0 || maxTimestamp != 0)
            {
                query = query.Where(x => x.CreatedAt > minTimestamp && x.CreatedAt < maxTimestamp);
            }

            var total = await query.CountAsync();

            query = query
                .Skip(offset)
                .Take(limit);

            // Создание выражения для сортировки
            var parameter = Expression.Parameter(typeof(Expense), orderBy);
            var property = Expression.Property(parameter, orderBy);
            var orderByExpression = Expression.Lambda<Func<Expense, object>>(Expression.Convert(property, typeof(object)), parameter);

            if (order == "desc")
            {
                query = query.OrderByDescending(orderByExpression);
            }
            else
            {
                query = query.OrderBy(orderByExpression);
            }

            var expenses = await query.ToArrayAsync();

            var categoryIds = expenses.Select(x => x.CategoryId).ToArray();
            var includedCategories = await _db.ExpenseCategories.Where(x => categoryIds.Contains(x.Id)).ToArrayAsync();

            var budgetIds = expenses.Select(x => x.BudgetId).ToArray();

            var includedBudgets = await _db.Budgets
                .Where(x => budgetIds.Contains(x.Id))
                .Include(x => x.Expenses)
                .Select(x => x.ConvertToDTO(timeZoneOffset))
                .ToArrayAsync();

            return new ExpensesListDTO
            {
                Data = expenses,
                Categories = includedCategories,
                Budgets = includedBudgets,
                Total = total
            };
        }

        public async Task<ExpenseDashboardDTO> GetDashboardData(Guid userId, long minTimestamp, long maxTimestamp, Guid?[] categories = null, int timezoneOffset = 0)
        {
            // Распределение расходов по категориям
            var sumByCategoryId = new Dictionary<Guid, decimal>();
            var includedCategories = new List<ExpenseCategory>();

            // Общая статистика
            decimal totalAmount = 0M;

            var query = _db.Expenses.AsNoTracking().Where(x => x.OwnerId == userId);
            if (categories != null && categories.Length > 0)
            {
                if (categories.Any(x => x.Value == Guid.Empty))
                {
                    query = query.Where(x => categories.Contains(x.CategoryId) || x.CategoryId == null);
                }
                else
                {
                    query = query.Where(x => categories.Contains(x.CategoryId));
                }
            }

            // Обработка наличия периода
            if (minTimestamp != 0 && maxTimestamp != 0 && maxTimestamp > minTimestamp)
            {
                query = query.Where(x => x.CreatedAt > minTimestamp && x.CreatedAt < maxTimestamp);
            }

            query = query
                .Include(x => x.Category)
                .OrderBy(x => x.CreatedAt);

            var expenses = await query.ToArrayAsync();
            if (expenses.Length == 0)
            {
                return new ExpenseDashboardDTO
                {
                    ChartData = [],
                    PieChartData = sumByCategoryId,
                    Categories = includedCategories
                };
            }

            long realMinTimestamp = expenses.First().CreatedAt;
            long realMaxTimestamp = expenses.Last().CreatedAt;

            long timeSegment = realMaxTimestamp - realMinTimestamp;
            var chartLabelFormat = "dd.MM.yyyy";


            if (timeSegment > 5184000 && timeSegment <= 61758000)
            {
                chartLabelFormat = "MM.yyyy";
            }
            else if (timeSegment > 61758000)
            {
                chartLabelFormat = "yyyy";
            }

            var chartSeriesDict = new Dictionary<DateTime, decimal>();
            foreach (var expense in expenses)
            {
                // Заполнение по временным отрезкам для гарфика
                var createdAt = DateTimeOffset
                .FromUnixTimeSeconds(expense.CreatedAt)
                .ToOffset(TimeSpan.FromHours(timezoneOffset))
                .DateTime;

                DateTime segmentKey;
                if (chartLabelFormat == "yyyy")
                {
                    segmentKey = new DateTime(createdAt.Year, 1, 1);
                }
                else if (chartLabelFormat == "MM.yyyy")
                {
                    segmentKey = new DateTime(createdAt.Year, createdAt.Month, 1);
                }
                else
                {
                    segmentKey = new DateTime(createdAt.Year, createdAt.Month, createdAt.Day);
                }

                if (!chartSeriesDict.ContainsKey(segmentKey))
                {
                    chartSeriesDict[segmentKey] = 0;
                }

                chartSeriesDict[segmentKey] += expense.Amount;
                //=

                // Заполнение распределения по категориям (для круговой диаграммы)
                var myCategoryId = expense.CategoryId == null ? Guid.Empty : expense.CategoryId;
                if (sumByCategoryId.ContainsKey(myCategoryId.Value))
                {
                    try
                    {
                        sumByCategoryId[myCategoryId.Value] += expense.Amount;
                    }
                    catch (OverflowException)
                    {
                        sumByCategoryId[myCategoryId.Value] = decimal.MaxValue;
                    }
                }
                else
                {
                    sumByCategoryId.Add(myCategoryId.Value, expense.Amount);
                }

                // Добавление в ответ информации о категории расхода
                if (expense.Category != null)
                {
                    includedCategories.Add(expense.Category);
                }

                try
                {
                    totalAmount += expense.Amount;
                }
                catch (OverflowException)
                {
                    totalAmount = decimal.MaxValue;
                }

            }

            var chartSegments = chartSeriesDict.Select(kv => new ChartSegment
            {
                x = kv.Key.ToString(chartLabelFormat),
                y = kv.Value
            }).ToArray();

            return new ExpenseDashboardDTO
            {
                TotalOperations = expenses.Length,
                TotalAmount = totalAmount,
                ChartData = chartSegments,
                PieChartData = sumByCategoryId,
                Categories = includedCategories
            };
        }
    }
}
