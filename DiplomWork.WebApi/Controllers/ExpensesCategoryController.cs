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
    [Route("api/expenses/category")]
    public class ExpensesCategoryController : ControllerBase
    {
        readonly ExpenseCategoryService _expenseCategoryService;

        public ExpensesCategoryController(ExpenseCategoryService expenseCategoryService)
        {
            _expenseCategoryService = expenseCategoryService;
        }

        [HttpGet]
        public async Task<EntityListDTO<ExpenseCategory>> GetCategories(int offset, int limit)
        {
            var userId = this.GetClaimsUserId(User).Value;

            return await _expenseCategoryService.GetUserExpenseCategories(userId, offset, Math.Min(limit, 100));
        }

        [HttpPost]
        public async Task<ActionResult<ExpenseCategory?>> AddCategory([FromBody]AddCategoryDTO category)
        {
            var validator = new AddCategoryValidator();
            if(!validator.Validate(category).IsValid)
            {
                return BadRequest();
            }

            var userId = this.GetClaimsUserId(User).Value;

            try
            {
                var newCategory = await _expenseCategoryService.AddExpenseCategory(category, userId);
                return Ok(newCategory);
            }
            catch(Exception ex){
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> EditCategory(Guid id, [FromBody]AddCategoryDTO category)
        {
            var validator = new AddCategoryValidator();
            if (!validator.Validate(category).IsValid)
            {
                return BadRequest();
            }

            var userId = this.GetClaimsUserId(User).Value;

            try
            {
                await _expenseCategoryService.EditExpenseCategory(id, category, userId);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCategory(Guid id, bool deleteChilds = false)
        {
            var userId = this.GetClaimsUserId(User).Value;

            await _expenseCategoryService.DeleteExpenseCategory(id, userId, deleteChilds);

            return Ok();
        }
    }
}
