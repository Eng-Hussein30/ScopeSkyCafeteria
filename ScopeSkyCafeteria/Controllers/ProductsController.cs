using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScopeSkyCafeteria.DTOs;
using ScopeSkyCafeteria.Models.Domain;
using ScopeSkyCafeteria.Models.DTOs;
using ScopeSkyCafeteria.Repositories;

namespace ScopeSkyCafeteria.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin")]

    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository productRepository;
        private readonly IMapper mapper;
        private readonly IWebHostEnvironment webHostEnvironment;

        public ProductsController(IProductRepository productRepository, IMapper mapper, IWebHostEnvironment webHostEnvironment)
        {
            this.productRepository = productRepository;
            this.mapper = mapper;
            this.webHostEnvironment = webHostEnvironment;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            var productDomain = mapper.Map<Product>(createProductDto);

            await productRepository.CreateProductAsync(productDomain);

            return Ok(mapper.Map<ProductDto>(productDomain));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts(
            [FromQuery] Guid? categoryId,
            [FromQuery] string? search)
        {
            var products = await productRepository.GetAllProductsAsync(categoryId, search);

            return Ok(mapper.Map<List<ProductDto>>(products));
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

        [HttpPost("{id:guid}/upload-image")]
        [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage(Guid id, [FromForm] UploadProductImageDTO uploadProductImageDTO)
        {
            if (uploadProductImageDTO.Image == null || uploadProductImageDTO.Image.Length == 0)
            {
                return BadRequest("Please select an image.");
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadProductImageDTO.Image.FileName);

            var filePath = Path.Combine(
                webHostEnvironment.WebRootPath,
                "images",
                "products",
                fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await uploadProductImageDTO.Image.CopyToAsync(stream);
            }

            var imageUrl = "/images/products/" + fileName;

            var updatedProduct = await productRepository.UpdateProductImageAsync(id, imageUrl);

            if (updatedProduct == null)
            {
                return NotFound("Product not found.");
            }

            return Ok(new
            {
                Message = "Image uploaded successfully",
                ImageUrl = imageUrl
            });
        }

    }
}