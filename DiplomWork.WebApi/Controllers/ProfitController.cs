using DiplomWork.Models;
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
    [Route("api/profit")]
    public class ProfitController : ControllerBase
    {
        readonly ProfitService _ProfitService;

        public ProfitController(ProfitService ProfitService)
        {
            _ProfitService = ProfitService;
        }

        [HttpGet]
        public async Task<ProfitListDTO> GetProfit(int offset, int limit, string? orderBy, string? order, long? minTimestamp, long? maxTimestamp, List<Guid?> categories)
        {
            var userId = this.GetClaimsUserId(User).Value;

            return await _ProfitService.GetUserProfits(userId, offset, Math.Min(limit, 100), orderBy, order, minTimestamp, maxTimestamp, categories);
        }

        [HttpPost]
        public async Task<ActionResult<Profit?>> AddProfit([FromBody]CreateProfitDTO Profit)
        {
            var validator = new CreateProfitDTOValidator();
            if(!validator.Validate(Profit).IsValid)
            {
                return BadRequest();
            }

            var userId = this.GetClaimsUserId(User).Value;

            var newProfit = await _ProfitService.AddNewProfit(Profit, userId);

            return Ok(newProfit);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> EditProfit(Guid id, [FromBody] CreateProfitDTO Profit)
        {
            var validator = new CreateProfitDTOValidator();
            if (!validator.Validate(Profit).IsValid)
            {
                return BadRequest();
            }

            var userId = this.GetClaimsUserId(User).Value;

            await _ProfitService.EditProfit(id, Profit, userId);

            return Ok();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteProfit(Guid id)
        {
            var userId = this.GetClaimsUserId(User).Value;

            await _ProfitService.DeleteProfitById(id);

            return Ok();
        }
    }
}
