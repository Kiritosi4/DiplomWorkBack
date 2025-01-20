using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DiplomWork.DTO;
using DiplomWork.WebApi.Validators;
using DiplomWork.Application.Services;
using DiplomWork.WebApi.Extensions;

namespace DiplomWork.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/targets")]
    public class TargetController : ControllerBase
    {
        readonly TargetService _TargetService;

        public TargetController(TargetService TargetService)
        {
            _TargetService = TargetService;
        }

        [HttpGet]
        public async Task<EntityListDTO<TargetDTO>> GetCategories(int offset, int limit = 25)
        {
            var userId = this.GetClaimsUserId(User).Value;

            return await _TargetService.GetUserTargetsList(userId, offset, Math.Min(limit, 100));
        }

        [HttpPost]
        public async Task<IActionResult> AddTarget([FromBody]AddTargetDTO Target)
        {
            var validator = new AddTargetValidator();
            if(!validator.Validate(Target).IsValid)
            {
                return BadRequest();
            }

            var userId = this.GetClaimsUserId(User).Value;

            var newTarget = await _TargetService.AddTarget(Target, userId);

            return Ok(newTarget);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> EditTarget(Guid id, [FromBody]AddTargetDTO Target)
        {
            var validator = new AddTargetValidator();
            if (!validator.Validate(Target).IsValid)
            {
                return BadRequest();
            }

            var userId = this.GetClaimsUserId(User).Value;

            await _TargetService.EditTarget(id, Target, userId);

            return Ok();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteTarget(Guid id, bool deleteChilds = false)
        {
            var userId = this.GetClaimsUserId(User).Value;

            await _TargetService.DeleteTarget(id, userId);

            return Ok();
        }
    }
}
