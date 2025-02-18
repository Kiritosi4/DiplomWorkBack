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
    [Route("api/budgets")]
    public class BudgetController : ControllerBase
    {
        readonly BudgetService _BudgetService;

        public BudgetController(BudgetService BudgetService)
        {
            _BudgetService = BudgetService;
        }

        [HttpGet]
        public async Task<EntityListDTO<BudgetDTO>> GetCategories(int offset, int limit = 25)
        {
            var userId = this.GetClaimsUserId(User).Value;

            return await _BudgetService.GetUserBudgetDTOList(userId, offset, Math.Min(limit, 100));
        }

        [HttpPost]
        public async Task<IActionResult> AddBudget([FromBody]AddBudgetDTO budget)
        {
            var validator = new AddBudgetValidator();
            if(!validator.Validate(budget).IsValid)
            {
                return BadRequest();
            }

            var userId = this.GetClaimsUserId(User).Value;
            try
            {
                var newBudget = await _BudgetService.AddBudget(budget, userId);
                return Ok(newBudget);
            }catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<BudgetDTO?>> EditBudget(Guid id, [FromBody]AddBudgetDTO budget)
        {
            var validator = new AddBudgetValidator();
            if (!validator.Validate(budget).IsValid)
            {
                return BadRequest("Ошибка валидации");
            }

            var userId = this.GetClaimsUserId(User).Value;

            try
            {
                return Ok(await _BudgetService.EditBudget(id, budget, userId));
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteBudget(Guid id, bool deleteChilds = false)
        {
            var userId = this.GetClaimsUserId(User).Value;

            await _BudgetService.DeleteBudget(id, userId);

            return Ok();
        }

        [HttpGet("{id:guid}/expenses")]
        public async Task<ActionResult<EntityListDTO<Expense>>> GetBudgetExpenses(Guid id, int offset = 0, int limit = 10)
        {
            var userId = this.GetClaimsUserId(User).Value;

            try
            {
                return Ok(await _BudgetService.GetBudgetExpenses(userId, id, offset, Math.Min(10, limit)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
