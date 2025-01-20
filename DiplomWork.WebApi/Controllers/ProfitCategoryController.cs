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
    [Route("api/profit/category")]
    public class ProfitCategoryController : ControllerBase
    {
        readonly ProfitCategoryService _profitCategoryService;

        public ProfitCategoryController(ProfitCategoryService profitCategoryService)
        {
            _profitCategoryService = profitCategoryService;
        }

        [HttpGet]
        public async Task<EntityListDTO<ProfitCategory>> GetCategories(int offset, int limit)
        {
            var userId = this.GetClaimsUserId(User).Value;

            return await _profitCategoryService.GetUserProfitCategories(userId, offset, Math.Min(limit, 100));
        }

        [HttpPost]
        public async Task<ActionResult<ProfitCategory?>> AddCategory([FromBody]AddCategoryDTO category)
        {
            var validator = new AddCategoryValidator();
            if(!validator.Validate(category).IsValid)
            {
                return BadRequest();
            }

            var userId = this.GetClaimsUserId(User).Value;

            var newCategory = await _profitCategoryService.AddProfitCategory(category, userId);

            return Ok(newCategory);
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

            await _profitCategoryService.EditProfitCategory(id, category, userId);

            return Ok();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCategory(Guid id, bool deleteChilds = false)
        {
            var userId = this.GetClaimsUserId(User).Value;

            await _profitCategoryService.DeleteProfitCategory(id, userId, deleteChilds);

            return Ok();
        }
    }
}
