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
            // Подготовка временных отрезков
            var diff = maxTimestamp - minTimestamp;

            var chartSeriesDict = new Dictionary<long, decimal>();

            // Распределение расходов по категориям
            var sumByCategoryId = new Dictionary<Guid, decimal>();
            var includedCategories = new List<ExpenseCategory>();

            // Общая статистика
            decimal totalAmount = 0M;
            //decimal lastTotalAmount = 0M;
            //int lastTotalOperations = 0;

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

                /*
                var query2 = query.Where(x => x.CreatedAt > minTimestamp - diff && x.CreatedAt < maxTimestamp - diff);

                try
                {
                    lastTotalAmount = await query2.SumAsync(x => x.Amount);
                }
                catch (OverflowException)
                {
                    lastTotalAmount = decimal.MaxValue;
                }

                try
                {
                    lastTotalOperations = await query2.CountAsync();
                }
                catch (OverflowException)
                {
                    lastTotalOperations = int.MaxValue;
                }
                */
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
            var startDateTime = DateTimeOffset.FromUnixTimeSeconds(realMinTimestamp).UtcDateTime;
            long realMaxTimestamp = expenses.Last().CreatedAt;
            var endDateTime = DateTimeOffset.FromUnixTimeSeconds(realMaxTimestamp).UtcDateTime;

            long timeSegment = realMaxTimestamp - realMinTimestamp;
            var chartLabelFormat = "dd.MM.yyyy";

            startDateTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, 0, 0, 0);
            endDateTime = new DateTime(endDateTime.Year, endDateTime.Month, endDateTime.Day, 0, 0, 0);

            if (timeSegment > 5184000 && timeSegment <= 61758000)
            {
                timeSegment = 2592000;
                chartLabelFormat = "MM.yyyy";

                startDateTime = new DateTime(startDateTime.Year, startDateTime.Month, 1, 0, 0, 0);
                endDateTime = new DateTime(endDateTime.Year, endDateTime.Month, 1, 0, 0, 0);
            }
            else if (timeSegment > 61758000)
            {
                timeSegment = 30879000;
                chartLabelFormat = "yyyy";

                startDateTime = new DateTime(startDateTime.Year, 1, 1, 0, 0, 0);
                endDateTime = new DateTime(endDateTime.Year, 1, 1, 0, 0, 0);
            }
            else
            {
                timeSegment = 86400;
            }

            realMinTimestamp = new DateTimeOffset(startDateTime).ToUnixTimeSeconds() - timezoneOffset * 3600;
            realMaxTimestamp = new DateTimeOffset(endDateTime).ToUnixTimeSeconds() - timezoneOffset * 3600;

            foreach (var expense in expenses)
            {
                // Заполнение по временным отрезкам
                var segmentId = (expense.CreatedAt - realMinTimestamp) / timeSegment;
                var mySegment = realMinTimestamp + timeSegment * segmentId;
                if (chartSeriesDict.ContainsKey(mySegment))
                {
                    try
                    {
                        chartSeriesDict[mySegment] += expense.Amount;
                    }
                    catch (OverflowException)
                    {
                        chartSeriesDict[mySegment] = decimal.MaxValue;
                    }

                }
                else
                {
                    chartSeriesDict.Add(mySegment, expense.Amount);
                }

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
                x = DateTimeOffset.FromUnixTimeSeconds(kv.Key + timezoneOffset * 3600).DateTime.ToString(chartLabelFormat),
                y = kv.Value,
            })
            .ToArray();

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
