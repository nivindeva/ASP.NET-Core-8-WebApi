using Intranet.Application.DTOs;
using Intranet.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Intranet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)] // Specify default content type
    [Consumes(MediaTypeNames.Application.Json)]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger; // Inject logger

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all products.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of products.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            _logger.LogInformation("API endpoint called: GetAll Products");
            var products = await _productService.GetAllProductsAsync(cancellationToken);
            return Ok(products);
        }

        /// <summary>
        /// Gets a specific product by ID.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The requested product.</returns>
        [HttpGet("{id:int}", Name = "GetProductById")] // Add name for CreatedAtAction
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("API endpoint called: GetProductById with ID {ProductId}", id);
            var product = await _productService.GetProductByIdAsync(id, cancellationToken);
            if (product == null)
            {
                _logger.LogWarning("Product not found for ID {ProductId}", id);
                return NotFound(); // Returns standard 404 Not Found response
            }
            return Ok(product);
        }

        /// <summary>
        /// Creates a new product.
        /// </summary>
        /// <param name="productDto">The product data transfer object.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created product.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateUpdateProductDto productDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) // Basic validation based on DTO attributes (e.g., [Required])
            {
                _logger.LogWarning("Invalid model state for creating product.");
                return BadRequest(ModelState);
            }

            _logger.LogInformation("API endpoint called: Create Product with name {ProductName}", productDto.Name);
            var createdProduct = await _productService.CreateProductAsync(productDto, cancellationToken);

            // Return 201 Created with a Location header pointing to the new resource
            return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
        }

        /// <summary>
        /// Updates an existing product.
        /// </summary>
        /// <param name="id">The ID of the product to update.</param>
        /// <param name="productDto">The updated product data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content if successful.</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateUpdateProductDto productDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for updating product ID {ProductId}.", id);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("API endpoint called: Update Product with ID {ProductId}", id);
            var success = await _productService.UpdateProductAsync(id, productDto, cancellationToken);
            if (!success)
            {
                // Could be NotFound or potentially a concurrency issue handled in the service/repo
                _logger.LogWarning("Product not found or failed to update for ID {ProductId}", id);
                return NotFound(); // Assuming failure means not found for simplicity here
            }
            return NoContent(); // Standard response for successful PUT/PATCH with no body
        }

        /// <summary>
        /// Deletes a product by ID.
        /// </summary>
        /// <param name="id">The ID of the product to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("API endpoint called: Delete Product with ID {ProductId}", id);
            var success = await _productService.DeleteProductAsync(id, cancellationToken);
            if (!success)
            {
                _logger.LogWarning("Product not found for deletion with ID {ProductId}", id);
                return NotFound();
            }
            return NoContent();
        }
    }
}
