using DiplomWork.Models;
using DiplomWork.Persistance;
using DiplomWork.Persistance.JWT;
using Microsoft.EntityFrameworkCore;

namespace DiplomWork.Application.Services
{
    public class AuthService
    {
        readonly DiplomWorkDbContext _db;
        readonly JwtProvider _jwtProvider;

        public AuthService(DiplomWorkDbContext db, JwtProvider jwtProvider)
        {
            _db = db;
            _jwtProvider = jwtProvider;
        }


        public async Task<string> Register(string email, string name, string password)
        {
            var userExists = await _db.Users.AnyAsync(x => x.Email == email);
            if (userExists)
            {
                throw new Exception("Пользователь уже зарегистрирован");
            }

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                Name = name,
                Password = PasswordHasher.HashPassword(password)
            };

            await _db.Users.AddAsync(newUser);
            await _db.SaveChangesAsync();

            return _jwtProvider.GetToken(newUser);
        }

        public async Task<string> Login(string email, string password)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == email);
            if(user == null || !PasswordHasher.VerifyPassword(password, user.Password))
            {
                throw new Exception("Неверный логин или пароль");
            }

            return _jwtProvider.GetToken(user);
        }
    }
}
