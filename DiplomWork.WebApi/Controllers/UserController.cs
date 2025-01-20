using DiplomWork.Application.Services;
using DiplomWork.Models;
using DiplomWork.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiplomWork.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }


        [HttpGet("me")]
        public async Task<User> Me()
        {
            var userId = this.GetClaimsUserId(User).Value;
            return await _userService.GetUserById(userId);
        }

        [HttpGet("logout")]
        public async Task Logout()
        {
            Response.Cookies.Delete("Auth");
        }
    }
}
