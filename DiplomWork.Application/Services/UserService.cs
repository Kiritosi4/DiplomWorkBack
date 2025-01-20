using DiplomWork.Models;
using DiplomWork.Persistance;
using Microsoft.EntityFrameworkCore;

namespace DiplomWork.Application.Services
{
    public class UserService
    {
        readonly DiplomWorkDbContext _db;
        readonly ExpensesService _expensesService;
        readonly ProfitService _profitService;

        public UserService(ExpensesService expensesService, ProfitService profitService, DiplomWorkDbContext diplomWorkDbContext)
        {
            _expensesService = expensesService;
            _profitService = profitService;
            _db = diplomWorkDbContext;
        }


        public async Task GetDashboard(Guid userId)
        {

        }

        public async Task<User> GetUserById(Guid userId)
        {
            return await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);    
        }
    }
}
