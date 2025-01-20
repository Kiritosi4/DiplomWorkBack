using DiplomWork.DTO;
using DiplomWork.Persistance;
using Microsoft.EntityFrameworkCore;
using DiplomWork.Models;

namespace DiplomWork.Application.Services
{
    public class ProfitCategoryService
    {
        DiplomWorkDbContext _db;

        public ProfitCategoryService(DiplomWorkDbContext db)
        {
            _db = db;
        }


        public async Task<ProfitCategory> AddProfitCategory(AddCategoryDTO addCategoryDTO, Guid userId)
        {
            if (await _db.ProfitCategories.AnyAsync(x => x.OwnerID == userId && x.Name == addCategoryDTO.Name))
            {
                throw new Exception("Категория с таким названием уже сущетвует.");
            }

            var newCategory = new ProfitCategory
            {
                Id = Guid.NewGuid(),
                Name = addCategoryDTO.Name,
                OwnerID = userId
            };

            await _db.ProfitCategories.AddAsync(newCategory);
            await _db.SaveChangesAsync();

            return newCategory;
        }

        public async Task DeleteProfitCategory(Guid categoryId, Guid userId, bool deleteChilds)
        {
            await _db.ProfitCategories.Where(x => x.Id == categoryId && x.OwnerID == userId).ExecuteDeleteAsync();

            if(deleteChilds)
            {
                await _db.Profits.Where(x => x.CategoryId == categoryId && x.OwnerId == userId).ExecuteDeleteAsync();
            }
        }

        public async Task EditProfitCategory(Guid categoryId, AddCategoryDTO editedCategory, Guid userId)
        {
            await _db.ProfitCategories
                .Where(x => x.Id == categoryId && x.OwnerID == userId)
                .ExecuteUpdateAsync(x => x
                .SetProperty(x => x.Name, editedCategory.Name));
        }

        public async Task<EntityListDTO<ProfitCategory>> GetUserProfitCategories(Guid userId, int offset = 0, int limit = 25)
        {
            var query = _db.ProfitCategories
                .Where(x => x.OwnerID == userId || x.OwnerID == null);

            var total = await query.CountAsync();

            var categories = await query
                .OrderBy(x => x.Name)
                .Skip(offset)
                .Take(limit)
                .ToArrayAsync();

            return new EntityListDTO<ProfitCategory>
            {
                Data = categories,
                Total = total
            };
        }
    }
}
