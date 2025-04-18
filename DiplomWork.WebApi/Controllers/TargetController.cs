using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DiplomWork.DTO;
using DiplomWork.WebApi.Validators;
using DiplomWork.Application.Services;
using DiplomWork.WebApi.Extensions;
using DiplomWork.Models;

namespace DiplomWork.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/targets")]
    public class TargetController : ControllerBase
    {
        readonly TargetService _targetService;

        public TargetController(TargetService TargetService)
        {
            _targetService = TargetService;
        }

        [HttpGet]
        public async Task<EntityListDTO<TargetDTO>> GetTargets(int offset, int limit = 25, string? filter = "all")
        {
            var userId = this.GetClaimsUserId(User).Value;

            return await _targetService.GetUserTargetsList(userId, offset, Math.Min(limit, 100), filter);
        }

        [HttpPost]
        public async Task<ActionResult<Target?>> AddTarget([FromBody]AddTargetDTO Target)
        {
            var validator = new AddTargetValidator();
            if(!validator.Validate(Target).IsValid)
            {
                return BadRequest();
            }

            var userId = this.GetClaimsUserId(User).Value;

            var newTarget = await _targetService.AddTarget(Target, userId);

            return Ok(newTarget);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<TargetDTO?>> EditTarget(Guid id, [FromBody]AddTargetDTO Target)
        {
            var validator = new AddTargetValidator();
            if (!validator.Validate(Target).IsValid)
            {
                return BadRequest();
            }

            var userId = this.GetClaimsUserId(User).Value;


            return Ok(await _targetService.EditTarget(id, Target, userId));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteTarget(Guid id, bool deleteChilds = false)
        {
            var userId = this.GetClaimsUserId(User).Value;

            await _targetService.DeleteTarget(id, userId);

            return Ok();
        }
    }
}
