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
    [Route("api/expenses")]
    public class ExpensesController : ControllerBase
    {
        readonly ExpensesService _expensesService;

        public ExpensesController(ExpensesService expensesService)
        {
            _expensesService = expensesService;
        }

        [HttpGet]
        public async Task<ExpensesListDTO> GetExpenses(int offset, int limit, string? orderBy, string? order, long? minTimestamp, long? maxTimestamp, [FromQuery]Guid?[] categories = null, int timezone = 0)
        {
            var userId = this.GetClaimsUserId(User).Value;

            return await _expensesService.GetUserExpenses(userId, offset, Math.Min(limit, 100), orderBy, order, minTimestamp, maxTimestamp, categories, timezone);
        }

        [HttpPost]
        public async Task<ActionResult<Expense?>> AddExpense([FromBody]CreateExpenseDTO expense)
        {
            var validator = new CreateExpenseDTOValidator();
            if(!validator.Validate(expense).IsValid)
            {
                return BadRequest("Ошибка валидации");
            }

            var userId = this.GetClaimsUserId(User).Value;

            var newExpense = await _expensesService.AddNewExpense(expense, userId);

            return Ok(newExpense);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> EditExpense(Guid id, [FromBody] CreateExpenseDTO expense)
        {
            var validator = new CreateExpenseDTOValidator();
            if (!validator.Validate(expense).IsValid)
            {
                return BadRequest();
            }

            var userId = this.GetClaimsUserId(User).Value;

            await _expensesService.EditExpense(id, expense, userId);

            return Ok();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteExpense(Guid id)
        {
            var userId = this.GetClaimsUserId(User).Value;

            await _expensesService.DeleteExpenseById(id);

            return Ok();
        }

        [HttpGet("dashboard")]
        public async Task<ExpenseDashboardDTO> GetExpensesDashboard(long minTimestamp, long maxTimestamp, [FromQuery] Guid?[] categories = null, int timezoneOffset = 0)
        {
            var userId = this.GetClaimsUserId(User).Value;

            return await _expensesService.GetDashboardData(userId, minTimestamp, maxTimestamp, categories, timezoneOffset);
        }
    }
}
