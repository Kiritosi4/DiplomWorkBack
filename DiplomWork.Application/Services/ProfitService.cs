using DiplomWork.Models;
using DiplomWork.Persistance;
using Microsoft.EntityFrameworkCore;
using DiplomWork.DTO;
using System.Linq.Expressions;

namespace DiplomWork.Application.Services
{
    public class ProfitService
    {
        readonly DiplomWorkDbContext _db;

        public ProfitService(DiplomWorkDbContext db)
        {
            _db = db;
        }

        public async Task<Profit> AddNewProfit(CreateProfitDTO createProfitDTO, Guid userId)
        {
            var newProfit = new Profit
            {
                Id = Guid.NewGuid(),
                Amount = createProfitDTO.Amount,
                CreatedAt = createProfitDTO.Timestamp,
                TargetId = createProfitDTO.TargetId,
                CategoryId = createProfitDTO.CategoryId,
                OwnerId = userId
            };
            await _db.Profits.AddAsync(newProfit);
            await _db.SaveChangesAsync();   

            return newProfit;
        }

        public async Task DeleteProfitById(Guid id)
        {
            await _db.Profits.Where(x => x.Id == id).ExecuteDeleteAsync();
        }

        public async Task EditProfit(Guid id, CreateProfitDTO editedProfit, Guid userId)
        {
            await _db.Profits
                .Where(x => x.Id == id && x.OwnerId == userId)
                .ExecuteUpdateAsync(x => x
                .SetProperty(x => x.TargetId, editedProfit.TargetId)
                .SetProperty(x => x.Amount, editedProfit.Amount)
                .SetProperty(x => x.CreatedAt, editedProfit.Timestamp)
                .SetProperty(x => x.CategoryId, editedProfit.CategoryId));
        }

        public async Task<ProfitListDTO> GetUserProfits(Guid userId, int offset = 0, int limit = 10, string orderBy = "date", string order = "desc", long? minTimestamp = 0, long? maxTimestamp = 0, List<Guid?> categories = null)
        {
            var query = _db.Profits
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
            var parameter = Expression.Parameter(typeof(Profit), orderBy);
            var property = Expression.Property(parameter, order);
            var orderByExpression = Expression.Lambda<Func<Profit, object>>(Expression.Convert(property, typeof(object)), parameter);

            if (order == "desc")
            {
                query = query.OrderByDescending(orderByExpression);
            }
            else
            {
                query = query.OrderBy(orderByExpression);
            }

            var profits = await query.ToArrayAsync();
            var categoryIds = profits.Select(x => x.CategoryId).ToArray();
            var includedCategories = await _db.ProfitCategories.Where(x => categoryIds.Contains(x.Id)).ToArrayAsync();

            return new ProfitListDTO
            {
                Data = profits,
                Categories = includedCategories,
                Total = total,
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
