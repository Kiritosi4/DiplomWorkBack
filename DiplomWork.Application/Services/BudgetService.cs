using DiplomWork.DTO;
using DiplomWork.Models;
using DiplomWork.Persistance;
using Microsoft.EntityFrameworkCore;

namespace DiplomWork.Application.Services
{
    public class BudgetService
    {
        DiplomWorkDbContext _db;

        public BudgetService(DiplomWorkDbContext db)
        {
            _db = db;
        }


        public async Task<Budget> AddBudget(AddBudgetDTO addBudgetDTO, Guid userId)
        {
            var period = (Period)addBudgetDTO.PeriodType;
            if (addBudgetDTO.Category != null && await _db.Budgets.AnyAsync(x => x.OwnerId == userId && x.CategoryId == addBudgetDTO.Category))
            {
                throw new Exception("Бюджет на выбранную категорию и период уже существует.");
            }

            var newBudget = new Budget
            {
                Id = Guid.NewGuid(),
                CategoryId = addBudgetDTO.Category,
                Limit = addBudgetDTO.Limit,
                Name = addBudgetDTO.Name,
                OwnerId = userId,
                PeriodType = period
            };

            await _db.Budgets.AddAsync(newBudget);
            await _db.SaveChangesAsync();

            return newBudget;
        }

        public async Task DeleteBudget(Guid budgetId, Guid userId)
        {
            await _db.Budgets.Where(x => x.Id == budgetId && x.OwnerId == userId).ExecuteDeleteAsync();
        }

        public async Task<BudgetDTO?> EditBudget(Guid budgetId, AddBudgetDTO editedBudget, Guid userId)
        {
            if (editedBudget.Category != null && await _db.Budgets.AnyAsync(x => x.Id != budgetId && x.OwnerId == userId && x.CategoryId == editedBudget.Category))
            {
                throw new Exception("Бюджет на выбранную категорию уже существует.");
            }

            await _db.Budgets
                .Where(x => x.Id == budgetId && x.OwnerId == userId)
                .ExecuteUpdateAsync(x => x
                .SetProperty(x => x.Name, editedBudget.Name)
                .SetProperty(x => x.CategoryId, editedBudget.Category)
                .SetProperty(x => x.PeriodType, (Period)editedBudget.PeriodType)
                .SetProperty(x => x.Limit, editedBudget.Limit));

            return await _db.Budgets
                .Where(x => x.Id == budgetId && x.OwnerId == userId)
                .Include(x => x.Expenses)
                .Select(x => x.ConvertToDTO())
                .FirstOrDefaultAsync();
        }

        public async Task<EntityListDTO<BudgetDTO>> GetUserBudgetDTOList(Guid userId, int offset = 0, int limit = 25)
        {
            var query = _db.Budgets
                .AsNoTracking()
                .Where(x => x.OwnerId == userId);

            var total = await query.CountAsync();

            var budgets = await query
                .Include(x => x.Expenses)
                .Select(x => x.ConvertToDTO())
                .ToArrayAsync();

            var filteredBudgets = budgets
                .OrderByDescending(x => x.Amount)
                .Skip(offset)
                .Take(limit)
                .ToArray();

            return new EntityListDTO<BudgetDTO>
            {
                Data = filteredBudgets,
                Total = total
            };
        }

        public async Task<ExpensesListDTO> GetBudgetExpenses(Guid userId, Guid budgetId, int offset = 0, int limit = 10)
        {
            var budget = await _db.Budgets.AsNoTracking().FirstOrDefaultAsync(x => x.Id == budgetId && x.OwnerId == userId);
            if (budget == null)
            {
                throw new Exception("Бюджет не найден");
            }

            var minTimestamp = Budget.GetStartOfPeriod(budget.PeriodType);
            var maxTimestamp = Budget.GetEndOfPeriod(budget.PeriodType);

            var query = _db.Expenses
                .AsNoTracking()
                .Where(x => x.OwnerId == userId && x.BudgetId == budgetId && x.CreatedAt > minTimestamp && x.CreatedAt < maxTimestamp);

            var total = await query.CountAsync();

            query = query
                .Skip(offset)
                .Take(limit);

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
    }
}
