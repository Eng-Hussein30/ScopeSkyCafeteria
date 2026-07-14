using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ScopeSkyCafeteria.DTOs;
using ScopeSkyCafeteria.Models.Domain;
using ScopeSkyCafeteria.Models.DTOs;
using ScopeSkyCafeteria.Repositories;

namespace ScopeSkyCafeteria.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository productRepository;
        private readonly IMapper mapper;

        public ProductsController(IProductRepository productRepository, IMapper mapper)
        {
            this.productRepository = productRepository;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            var productDomain = mapper.Map<Product>(createProductDto);

            await productRepository.CreateProductAsync(productDomain);

            return Ok(mapper.Map<ProductDto>(productDomain));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await productRepository.GetAllProductsAsync();

            var productsDto = mapper.Map<List<ProductDto>>(products);

            return Ok(productsDto);
        }


        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var product = await productRepository.GetProductByIdAsync(id);

            if (product == null)
                return NotFound("Product not found");

            var productDto = mapper.Map<ProductDto>(product);

            return Ok(productDto);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateProduct([FromRoute] Guid id, [FromBody] UpdateProductsDTO updateProductsDTO)
        {
            var productDomain = mapper.Map<Product>(updateProductsDTO);

            var updatedProduct = await productRepository.UpdateProductAsync(id, productDomain);

            if (updatedProduct == null) { return NotFound("Product not found"); }

            var productDto = mapper.Map<ProductDto>(updatedProduct);

            return Ok(productDto);
        }

        [HttpDelete]
        [Route("{id:guid}")]
        public async Task<IActionResult> DeleteProduct([FromRoute] Guid id)
        {
            var deletedProduct = await productRepository.DeleteProductAsync(id);
            if (deletedProduct == null) { return NotFound("Product not found"); }
            var productDto = mapper.Map<ProductDto>(deletedProduct);
            return Ok(productDto);
        }
    }
}