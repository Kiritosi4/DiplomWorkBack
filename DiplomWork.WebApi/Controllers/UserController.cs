using DiplomWork.Application.Services;
using DiplomWork.DTO;
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
        public async Task<UserDTO> Me()
        {
            var userId = this.GetClaimsUserId(User).Value;
            var user = await _userService.GetUserById(userId);
            return user.ConvertToDTO();
        }

        [HttpGet("logout")]
        public async Task Logout()
        {
            Response.Cookies.Delete("Auth");
        }

        [HttpGet("summary-amount")]
        public async Task<decimal> GetSummaryAmount()
        {
            var userId = this.GetClaimsUserId(User).Value;
            return await _userService.GetSummaryAmount(userId);
        }
    }
}
