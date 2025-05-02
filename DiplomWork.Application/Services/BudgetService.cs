using DiplomWork.DTO;
using DiplomWork.Models;
using DiplomWork.Persistance;
using Microsoft.EntityFrameworkCore;
using System;

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
            if (addBudgetDTO.CategoryId != null && await _db.Budgets.AnyAsync(x => x.OwnerId == userId && x.CategoryId == addBudgetDTO.CategoryId))
            {
                throw new Exception("Бюджет на выбранную категорию уже существует.");
            }

            var newBudget = new Budget
            {
                Id = Guid.NewGuid(),
                CategoryId = addBudgetDTO.CategoryId,
                Limit = addBudgetDTO.Limit,
                Name = addBudgetDTO.Name,
                OwnerId = userId,
                PeriodType = period,
                StartPeriod = addBudgetDTO.StartPeriod,
                EndPeriod = addBudgetDTO.EndPeriod
            };

            await _db.Budgets.AddAsync(newBudget);
            await _db.SaveChangesAsync();

            return newBudget;
        }

        public async Task DeleteBudget(Guid budgetId, Guid userId)
        {
            await _db.Budgets.Where(x => x.Id == budgetId && x.OwnerId == userId).ExecuteDeleteAsync();
        }

        public async Task<BudgetDTO?> EditBudget(Guid budgetId, AddBudgetDTO editedBudget, Guid userId, int timezone = 0)
        {
            if (editedBudget.CategoryId != null && await _db.Budgets.AnyAsync(x => x.Id != budgetId && x.OwnerId == userId && x.CategoryId == editedBudget.CategoryId))
            {
                throw new Exception("Бюджет на выбранную категорию уже существует.");
            }

            await _db.Budgets
                .Where(x => x.Id == budgetId && x.OwnerId == userId)
                .ExecuteUpdateAsync(x => x
                .SetProperty(x => x.Name, editedBudget.Name)
                .SetProperty(x => x.CategoryId, editedBudget.CategoryId)
                .SetProperty(x => x.PeriodType, (Period)editedBudget.PeriodType)
                .SetProperty(x => x.Limit, editedBudget.Limit)
                .SetProperty(x => x.StartPeriod, editedBudget.StartPeriod)
                .SetProperty(x => x.EndPeriod, editedBudget.EndPeriod)
                );


            return await _db.Budgets
                .Where(x => x.Id == budgetId && x.OwnerId == userId)
                .Include(x => x.Expenses)
                .Select(x => x.ConvertToDTO(timezone))
                .FirstOrDefaultAsync();
        }

        public async Task<BudgetListDTO> GetUserBudgetDTOList(Guid userId, int offset = 0, int limit = 25, int timezone = 0)
        {
            var query = _db.Budgets
                .AsNoTracking()
                .Where(x => x.OwnerId == userId);

            var total = await query.CountAsync();

            var budgets = await query
                .Include(x => x.Expenses)
                .Select(x => x.ConvertToDTO(timezone))
                .ToArrayAsync();

            var filteredBudgets = budgets
                .OrderByDescending(x => x.Amount)
                .Skip(offset)
                .Take(limit)
                .ToArray();

            var categoryIds = filteredBudgets.Select(x => x.CategoryId).ToArray();
            var incluidedCategories = await _db.ExpenseCategories.Where(x => categoryIds.Contains(x.Id)).ToArrayAsync();

            return new BudgetListDTO
            {
                Data = filteredBudgets,
                Total = total,
                Categories = incluidedCategories
            };
        }

        public async Task<ExpensesListDTO> GetBudgetExpenses(Guid userId, Guid budgetId, int offset = 0, int limit = 10, int timezone = 0)
        {
            var budget = await _db.Budgets.AsNoTracking().FirstOrDefaultAsync(x => x.Id == budgetId && x.OwnerId == userId);
            if (budget == null)
            {
                throw new Exception("Бюджет не найден");
            }

            var minTimestamp = budget.StartPeriod;
            var maxTimestamp = budget.EndPeriod;
            if(budget.PeriodType != Period.Custom)
            {
                minTimestamp = Budget.GetStartOfPeriod(budget.PeriodType, timezone);
                minTimestamp = Budget.GetEndOfPeriod(budget.PeriodType, timezone);
            }

            var query = _db.Expenses
                .AsNoTracking()
                .Where(x => x.OwnerId == userId && x.BudgetId == budgetId && x.CreatedAt >= minTimestamp && x.CreatedAt < maxTimestamp);

            int total = 0;
            try
            {
                total = await query.CountAsync();
            }
            catch (OverflowException)
            {
                total = int.MaxValue;
            }

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

        public async Task<BudgetDTO?> GetBudgetByCategory(Guid categoryId, Guid userId, int timezone = 0)
        {
            return await _db.Budgets
                .AsNoTracking()
                .Where(x => x.OwnerId == userId && x.CategoryId == categoryId)
                .Include(x => x.Expenses)
                .Select(x => x.ConvertToDTO(timezone))
                .FirstOrDefaultAsync();
        }
    }
}
