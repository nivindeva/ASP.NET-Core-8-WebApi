using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Intranet.Application.DTOs;
using Intranet.Application.Interfaces;
using Intranet.Domain.Entities;
using Intranet.Domain.Interfaces;
using Microsoft.Extensions.Logging;



namespace Intranet.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ProductService> _logger;
        private readonly IMapper _mapper; // Inject IMapper

        // Constructor updated for AutoMapper
        public ProductService(IProductRepository productRepository, ILogger<ProductService> logger, IMapper mapper)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper)); // Assign IMapper
        }

        public async Task<ProductDto> CreateProductAsync(CreateUpdateProductDto productDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to create a new product: {ProductName}", productDto.Name);
            // Map DTO to Entity using AutoMapper
            var product = _mapper.Map<Product>(productDto);

            var addedProduct = await _productRepository.AddAsync(product, cancellationToken);

            _logger.LogInformation("Product created successfully with ID: {ProductId}", addedProduct.Id);
            // Map Entity back to DTO using AutoMapper
            return _mapper.Map<ProductDto>(addedProduct);
        }

        public async Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to delete product with ID: {ProductId}", id);
            var result = await _productRepository.DeleteAsync(id, cancellationToken);
            if (result)
            {
                _logger.LogInformation("Product with ID: {ProductId} deleted successfully.", id);
            }
            else
            {
                _logger.LogWarning("Product with ID: {ProductId} not found for deletion.", id);
            }
            return result;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all products.");
            var products = await _productRepository.GetAllAsync(cancellationToken);
            // Map IEnumerable<Entity> to IEnumerable<DTO>
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching product with ID: {ProductId}", id);
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product == null)
            {
                _logger.LogWarning("Product with ID: {ProductId} not found.", id);
                return null;
            }
            // Map Entity to DTO
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<bool> UpdateProductAsync(int id, CreateUpdateProductDto productDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to update product with ID: {ProductId}", id);
            var existingProduct = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (existingProduct == null)
            {
                _logger.LogWarning("Product with ID: {ProductId} not found for update.", id);
                return false;
            }

            // Use AutoMapper to map updated fields from DTO onto the existing entity
            _mapper.Map(productDto, existingProduct);

            var result = await _productRepository.UpdateAsync(existingProduct, cancellationToken);
            if (result)
            {
                _logger.LogInformation("Product with ID: {ProductId} updated successfully.", id);
            }
            else
            {
                _logger.LogError("Failed to update product with ID: {ProductId}.", id);
            }
            return result;
        }
    }
}