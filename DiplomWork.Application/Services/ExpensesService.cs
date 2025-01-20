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

        public async Task<ExpensesListDTO> GetUserExpenses(Guid userId, int offset = 0, int limit = 10, string orderBy = "CreatedAt", string order = "desc", long? minTimestamp = 0, long? maxTimestamp = 0, List<Guid?> categories = null)
        {
            var query = _db.Expenses
                .AsNoTracking()
                .Where(x => x.OwnerId == userId);

            if(categories != null && categories.Count > 0)
            {
                query = query.Where(x => categories.Contains(x.CategoryId));
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
            var property = Expression.Property(parameter, order);
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
    }
}
