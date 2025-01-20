using DiplomWork.Application.Services;
using DiplomWork.WebApi.Contracts;
using DiplomWork.WebApi.Validators;
using Microsoft.AspNetCore.Mvc;

namespace DiplomWork.WebApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login([FromBody]LoginRequestBody loginRequestBody)
        {
            try
            {
                var loginResult = await _authService.Login(loginRequestBody.Email, loginRequestBody.Password);
                Response.Cookies.Append("Auth", loginResult);
                return Ok(loginResult);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<string>> Register([FromBody]RegisterRequestBody loginRequestBody)
        {
            var validator = new RegisterRequestValidator();
            if (!validator.Validate(loginRequestBody).IsValid)
            {
                return BadRequest("Ошибка валидации");
            }

            try
            {
                var regResult = await _authService.Register(loginRequestBody.Email, loginRequestBody.Name, loginRequestBody.Password);
                Response.Cookies.Append("Auth", regResult);
                return Ok(regResult);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
