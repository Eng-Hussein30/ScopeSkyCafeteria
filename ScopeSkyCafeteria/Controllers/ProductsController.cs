using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScopeSkyCafeteria.DTOs;
using ScopeSkyCafeteria.Models.Domain;
using ScopeSkyCafeteria.Models.DTOs;
using ScopeSkyCafeteria.Repositories;
using ScopeSkyCafeteria.Services.Interfaces;

namespace ScopeSkyCafeteria.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository productRepository;
        private readonly IMapper mapper;
        private readonly IFileStorageService fileStorageService;

        public ProductsController(
            IProductRepository productRepository,
            IMapper mapper,
            IFileStorageService fileStorageService)
        {
            this.productRepository = productRepository;
            this.mapper = mapper;
            this.fileStorageService = fileStorageService;
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
        public async Task<IActionResult> CreateProduct(
            [FromBody] CreateProductDto createProductDto)
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
            var products =
                await productRepository.GetAllProductsAsync(categoryId, search);

            return Ok(mapper.Map<List<ProductDto>>(products));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var product =
                await productRepository.GetProductByIdAsync(id);

            if (product == null)
                return NotFound("Product not found");

            var productDto =
                mapper.Map<ProductDto>(product);

            return Ok(productDto);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
        public async Task<IActionResult> UpdateProduct(
            [FromRoute] Guid id,
            [FromBody] UpdateProductsDTO updateProductsDTO)
        {
            var productDomain =
                mapper.Map<Product>(updateProductsDTO);

            var updatedProduct =
                await productRepository.UpdateProductAsync(id, productDomain);

            if (updatedProduct == null)
                return NotFound("Product not found");

            var productDto =
                mapper.Map<ProductDto>(updatedProduct);

            return Ok(productDto);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
        public async Task<IActionResult> DeleteProduct(
            [FromRoute] Guid id)
        {
            var deletedProduct =
                await productRepository.DeleteProductAsync(id);

            if (deletedProduct == null)
                return NotFound("Product not found");

            var productDto =
                mapper.Map<ProductDto>(deletedProduct);

            return Ok(productDto);
        }

        [HttpPost("{id:guid}/upload-image")]
        [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage(
            Guid id,
            [FromForm] UploadProductImageDTO uploadProductImageDTO)
        {
            if (uploadProductImageDTO.Image == null ||
                uploadProductImageDTO.Image.Length == 0)
            {
                return BadRequest("Please select an image.");
            }

            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            var extension =
                Path.GetExtension(uploadProductImageDTO.Image.FileName)
                    .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(
                    "Only JPG, JPEG, PNG and WEBP images are allowed.");
            }

            const long maxFileSize = 10 * 1024 * 1024;

            if (uploadProductImageDTO.Image.Length > maxFileSize)
            {
                return BadRequest(
                    "Maximum allowed image size is 10 MB.");
            }

            var allowedContentTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

            if (!allowedContentTypes.Contains(
                    uploadProductImageDTO.Image.ContentType))
            {
                return BadRequest("Invalid image format.");
            }

            var product =
                await productRepository.GetProductByIdAsync(id);

            if (product == null)
                return NotFound("Product not found.");

            // حفظ مسار الصورة القديمة قبل تحديث المنتج
            var oldImagePath = product.ImageUrl;

            // حفظ الصورة الجديدة في MinIO
            var newImagePath =
                await fileStorageService.SaveAsync(
                    uploadProductImageDTO.Image,
                    "products");

            // تحديث مسار الصورة الجديدة في قاعدة البيانات
            var updatedProduct =
                await productRepository.UpdateProductImageAsync(
                    id,
                    newImagePath);

            if (updatedProduct == null)
            {
                // إذا فشل تحديث قاعدة البيانات نحذف الصورة الجديدة
                await fileStorageService.DeleteAsync(newImagePath);

                return NotFound("Product not found.");
            }

            // حذف الصورة القديمة فقط
            if (!string.IsNullOrWhiteSpace(oldImagePath))
            {
                await fileStorageService.DeleteAsync(oldImagePath);
            }

            return Ok(new
            {
                Message = "Image uploaded successfully",
                ImageUrl = newImagePath
            });
        }
    }
}

