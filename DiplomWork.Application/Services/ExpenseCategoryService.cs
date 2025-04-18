using DiplomWork.DTO;
using DiplomWork.Persistance;
using Microsoft.EntityFrameworkCore;
using DiplomWork.Models;

namespace DiplomWork.Application.Services
{
    public class ExpenseCategoryService
    {
        DiplomWorkDbContext _db;

        public ExpenseCategoryService(DiplomWorkDbContext db)
        {
            _db = db;
        }


        public async Task<ExpenseCategory> AddExpenseCategory(AddCategoryDTO addCategoryDTO, Guid userId)
        {
            if (await _db.ExpenseCategories.AnyAsync(x => x.OwnerID == userId && x.Name == addCategoryDTO.Name))
            {
                throw new Exception("Категория с таким названием уже сущетвует.");
            }

            var newCategory = new ExpenseCategory
            {
                Id = Guid.NewGuid(),
                Name = addCategoryDTO.Name,
                OwnerID = userId
            };

            await _db.ExpenseCategories.AddAsync(newCategory);
            await _db.SaveChangesAsync();

            return newCategory;
        }

        public async Task DeleteExpenseCategory(Guid categoryId, Guid userId, bool deleteChilds)
        {
            await _db.ExpenseCategories.Where(x => x.Id == categoryId && x.OwnerID == userId).ExecuteDeleteAsync();

            if(deleteChilds)
            {
                await _db.Expenses.Where(x => x.CategoryId == categoryId && x.OwnerId == userId).ExecuteDeleteAsync();
            }
        }

        public async Task EditExpenseCategory(Guid categoryId, AddCategoryDTO editedCategory, Guid userId)
        {
            if (await _db.ExpenseCategories.AnyAsync(x => x.OwnerID == userId && x.Name == editedCategory.Name))
            {
                throw new Exception("Категория с таким названием уже сущетвует.");
            }

            await _db.ExpenseCategories
                .Where(x => x.Id == categoryId && x.OwnerID == userId)
                .ExecuteUpdateAsync(x => x
                .SetProperty(x => x.Name, editedCategory.Name));
        }

        public async Task<EntityListDTO<ExpenseCategory>> GetUserExpenseCategories(Guid userId, int offset = 0, int limit = 25)
        {
            var query = _db.ExpenseCategories
                .Where(x => x.OwnerID == userId || x.OwnerID == null);
                
            var total = await query.CountAsync();

            var categories = await query
                .OrderBy(x => x.Name)
                .Skip(offset)
                .Take(limit)
                .ToArrayAsync();

            return new EntityListDTO<ExpenseCategory>
            {
                Data = categories,
                Total = total
            };
        }
    }
}
