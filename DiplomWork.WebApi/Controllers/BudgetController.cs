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
        readonly BudgetService _budgetService;

        public BudgetController(BudgetService BudgetService)
        {
            _budgetService = BudgetService;
        }

        [HttpGet]
        public async Task<BudgetListDTO> GetBudgets(int offset, int limit = 25, int timezone = 0)
        {
            var userId = this.GetClaimsUserId(User).Value;

            return await _budgetService.GetUserBudgetDTOList(userId, offset, Math.Min(limit, 100), timezone);
        }

        [HttpPost]
        public async Task<ActionResult<Budget?>> AddBudget([FromBody]AddBudgetDTO budget)
        {
            var validator = new AddBudgetValidator();
            if(!validator.Validate(budget).IsValid)
            {
                return BadRequest("Ошибка валидации");
            }

            var userId = this.GetClaimsUserId(User).Value;
            try
            {
                var newBudget = await _budgetService.AddBudget(budget, userId);
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
                return Ok(await _budgetService.EditBudget(id, budget, userId, budget.TimezoneOffset));
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

            await _budgetService.DeleteBudget(id, userId);

            return Ok();
        }

        [HttpGet("{id:guid}/expenses")]
        public async Task<ActionResult<EntityListDTO<Expense>>> GetBudgetExpenses(Guid id, string? orderBy, string? order, int offset = 0, int limit = 10, int timezone = 0)
        {
            var userId = this.GetClaimsUserId(User).Value;

            try
            {
                return Ok(await _budgetService.GetBudgetExpenses(userId, id, offset, Math.Min(10, limit), orderBy, order, timezone));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("bycategory/{id:guid}")]
        public async Task<BudgetDTO?> GetBudgetByCategory(Guid id, int timezone = 0)
        {
            var userId = this.GetClaimsUserId(User).Value;
            return await _budgetService.GetBudgetByCategory(id, userId, timezone);
        }
    }
}
