using DiplomWork.DTO;
using DiplomWork.Persistance;
using DiplomWork.Models;
using Microsoft.EntityFrameworkCore;

namespace DiplomWork.Application.Services
{
    public class TargetService
    {
        DiplomWorkDbContext _db;

        public TargetService(DiplomWorkDbContext db)
        {
            _db = db;
        }


        public async Task<Target> AddTarget(AddTargetDTO addTargetDTO, Guid userId)
        {
            var newTarget = new Target
            {
                Id = Guid.NewGuid(),
                Limit = addTargetDTO.Limit,
                Name = addTargetDTO.Name,
                Amount = addTargetDTO.Amount,
                OwnerId = userId
            };

            await _db.Targets.AddAsync(newTarget);
            await _db.SaveChangesAsync();

            return newTarget;
        }

        public async Task DeleteTarget(Guid TargetId, Guid userId)
        {
            await _db.Targets.Where(x => x.Id == TargetId && x.OwnerId == userId).ExecuteDeleteAsync();
        }

        public async Task<TargetDTO?> EditTarget(Guid TargetId, AddTargetDTO editedTarget, Guid userId)
        {
            await _db.Targets
                .Where(x => x.Id == TargetId && x.OwnerId == userId)
                .ExecuteUpdateAsync(x => x
                .SetProperty(x => x.Name, editedTarget.Name)
                .SetProperty(x => x.Amount, editedTarget.Amount)
                .SetProperty(x => x.Limit, editedTarget.Limit)
                .SetProperty(x => x.Closed, editedTarget.Closed)
                );

            return await _db.Targets
                .Where(x => x.Id == TargetId && x.OwnerId == userId)
                .Select(x => x.ConvertToDTO())
                .FirstOrDefaultAsync();
        }

        public async Task<EntityListDTO<TargetDTO>> GetUserTargetsList(Guid userId, int offset = 0, int limit = 25, string? filter = "all")
        {
            var query = _db.Targets
                .AsNoTracking()
                .Where(x => x.OwnerId == userId);

            if(filter == "closed")
            {
                query = query.Where(x => x.Closed);
            }
            else if (filter == "opened")
            {
                query = query.Where(x => !x.Closed);
            }

            var total = await query.CountAsync();

            var targets = await query
                .Skip(offset)
                .Take(limit)
                .Select(x => x.ConvertToDTO())
                .ToArrayAsync();

            return new EntityListDTO<TargetDTO>
            {
                Data = targets,
                Total = total
            };
        }
    }
}
