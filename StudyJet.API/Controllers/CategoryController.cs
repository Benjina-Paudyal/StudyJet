using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyJet.API.DTOs.Category;
using StudyJet.API.Services.Interface;

namespace StudyJet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }


        [HttpGet]
        public async Task<ActionResult<List<CategoryResponseDTO>>> GetAllCategories()
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(categories);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryResponseDTO>> GetCategoryById(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound(); 

            return Ok(category);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<int>> AddCategory(CategoryRequestDTO categoryDto)
        {
            var categoryId = await _categoryService.AddAsync(categoryDto);
            return CreatedAtAction(nameof(GetCategoryById), new { id = categoryId }, categoryId);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateCategory(int id, CategoryRequestDTO categoryDto)
        {
            return BadRequest(new { message = "Category update is not allowed at the moment." });
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            return BadRequest(new { message = "Category deletion is not allowed at the moment." });
        }


        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> CategoryExists(int id)
        {
            var exists = await _categoryService.ExistsAsync(id);
            return Ok(exists); 
        }

    }
}
