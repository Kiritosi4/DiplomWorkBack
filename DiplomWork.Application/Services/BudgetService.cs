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
            if (addBudgetDTO.Category != null && await _db.Budgets.AnyAsync(x => x.CategoryId == addBudgetDTO.Category && x.PeriodType == period))
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

        public async Task EditBudget(Guid budgetId, AddBudgetDTO editedBudget, Guid userId)
        {
            await _db.Budgets
                .Where(x => x.Id == budgetId && x.OwnerId == userId)
                .ExecuteUpdateAsync(x => x
                .SetProperty(x => x.Name, editedBudget.Name)
                .SetProperty(x => x.CategoryId, editedBudget.Category)
                .SetProperty(x => x.PeriodType, (Period)editedBudget.PeriodType));
        }

        public async Task<EntityListDTO<BudgetDTO>> GetUserBudgetDTOList(Guid userId, int offset = 0, int limit = 25)
        {
            var query = _db.Budgets
                .AsNoTracking()
                .Where(x => x.OwnerId == userId);

            var total = await query.CountAsync();

            var budgets = await query
                .Skip(offset)
                .Take(limit)
                .Include(x => x.Expenses)
                .Select(x => x.ConvertToDTO())
                .ToArrayAsync();

            return new EntityListDTO<BudgetDTO>
            {
                Data = budgets,
                Total = total
            };
        }

    }
}
