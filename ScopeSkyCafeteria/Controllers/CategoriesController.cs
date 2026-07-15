using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ScopeSkyCafeteria.Models.Domain;
using ScopeSkyCafeteria.Models.DTOs;
using ScopeSkyCafeteria.Repositories;

namespace ScopeSkyCafeteria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository categoryRepository;
        private readonly IMapper mapper;
        public CategoriesController(IMapper mapper, ICategoryRepository categoryRepository)
        {
            this.categoryRepository = categoryRepository;
            this.mapper = mapper;
        }

        [HttpPost]

        public async Task<IActionResult> Create([FromBody] AddCategoriesDTO addCategoriesDTO)
        {
            var categoryDomain = mapper.Map<Category>(addCategoriesDTO);
            await categoryRepository.CreateCategoryAsync(categoryDomain);
            return Ok(mapper.Map<CategoriesDTO>(categoryDomain));
        }

        [HttpPut]
        [Route("{Id:guid}")]

        public async Task<IActionResult> Update([FromRoute] Guid Id, UpdateCategoriesDTO updateCategoriesDTO)
        {
            var categoryDomain = mapper.Map<Category>(updateCategoriesDTO);
            categoryDomain = await categoryRepository.UpdateCategoryAsync(Id, categoryDomain);
            if (categoryDomain == null) { return NotFound("Invalid category id"); }
            return Ok(mapper.Map<CategoriesDTO>(categoryDomain));
        }

        [HttpDelete]
        [Route("{Id:guid}")]

        public async Task<IActionResult> Delete([FromRoute] Guid Id)
        {
            var categoryDomain = await categoryRepository.DeleteCategoryAsync(Id);
            if (categoryDomain == null) { return NotFound("Invalid category id"); }

            return Ok(mapper.Map<CategoriesDTO>(categoryDomain));

        }



        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await categoryRepository.GetAllCategoryAsync();

            return Ok(mapper.Map<List<CategoriesDTO>>(categories));
        }

        [HttpGet]
        [Route("{Id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid Id)
        {
            var categoryDomain = await categoryRepository.GetByIdCategoryAsync(Id);
            if (categoryDomain == null) { return NotFound("Invalid category id"); }
            return Ok(mapper.Map<CategoriesDTO>(categoryDomain));
        }
    }
}
