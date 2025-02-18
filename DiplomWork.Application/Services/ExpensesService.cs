using DiplomWork.Models;
using DiplomWork.Persistance;
using Microsoft.EntityFrameworkCore;
using DiplomWork.DTO;
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

        public async Task<ExpensesListDTO> GetUserExpenses(Guid userId, int offset = 0, int limit = 10, string orderBy = "CreatedAt", string order = "desc", long? minTimestamp = 0, long? maxTimestamp = 0, Guid?[] categories = null, bool emptyCategory = false)
        {
            var query = _db.Expenses
                .AsNoTracking()
                .Where(x => x.OwnerId == userId);

            if(categories != null && categories.Length > 0)
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

            if(minTimestamp > 0 && maxTimestamp > 0)
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

            return new ExpensesListDTO
            {
                Data = expenses,
                Categories = includedCategories,
                Total = total
            };
        }

        static Func<TObject, TKey> BuildKeySelector<TObject, TKey>(string propertyName)
        {
            return obj =>
            {
                var prop = typeof(TObject).GetProperty(propertyName, typeof(TKey));
                return (TKey)prop.GetValue(obj);
            };
        }

        public async Task<ExpensesDashboardDTO> GetDashboardData(Guid userId, long minTimestamp, long maxTimestamp, Guid?[] categories = null)
        {
            // Подготовка временных отрезков
            var diff = maxTimestamp - minTimestamp;
            long timeSegment = 86400;
            if(timeSegment > 5184000)
            {
                timeSegment = 2592000;
            }
            else if(timeSegment > 61758000)
            {
                timeSegment = 30879000;
            }

            var chartSeriesDict = new Dictionary<long, decimal>();

            // Распределение расходов по категориям
            var sumByCategoryId = new Dictionary<Guid, decimal>();
            var includedCategories = new List<ExpenseCategory>();

            // Общая статистика
            decimal totalAmount = 0;
            var totalOperations = 0;

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

            var query2 = query.Where(x => x.CreatedAt > minTimestamp - diff && x.CreatedAt < maxTimestamp - diff);
            query = query
                .Where(x => x.CreatedAt > minTimestamp && x.CreatedAt < maxTimestamp)
                .Include(x => x.Category)
                .OrderBy(x => x.CreatedAt);

            await query.ForEachAsync(expense =>
            {
                // Заполнение по временным отрезкам
                var segmentAmount = (expense.CreatedAt - minTimestamp) / timeSegment;
                var mySegment = minTimestamp + timeSegment * segmentAmount;
                if (chartSeriesDict.ContainsKey(mySegment))
                {
                    chartSeriesDict[mySegment] += expense.Amount;
                }
                else
                {
                    chartSeriesDict.Add(mySegment, expense.Amount);
                }

                // Заполнение распределения по категориям
                var myCategoryId = expense.CategoryId == null ? Guid.Empty : expense.CategoryId;
                if (sumByCategoryId.ContainsKey(myCategoryId.Value))
                {
                    sumByCategoryId[myCategoryId.Value] += expense.Amount;
                }
                else
                {
                    sumByCategoryId.Add(myCategoryId.Value, expense.Amount);
                }

                if (expense.Category != null)
                {
                    includedCategories.Add(expense.Category);
                }

                totalAmount += expense.Amount;
                totalOperations++;
            });

            var chartSegments = chartSeriesDict.Select(kv => new ChartSegment
            {
                x = DateTimeOffset.FromUnixTimeSeconds(kv.Key).DateTime.ToString("dd.MM.yyyy"),
                y = kv.Value,
            })
            .ToArray();

            return new ExpensesDashboardDTO
            {
                TotalOperations = totalOperations,
                TotalAmount = totalAmount,
                ChartData = chartSegments,
                PieChartData = sumByCategoryId,
                LastTotalAmount = await query2.SumAsync(x => x.Amount),
                LastTotalOperations = await query2.CountAsync(),
                Categories = includedCategories
            };
        }
    }
}
