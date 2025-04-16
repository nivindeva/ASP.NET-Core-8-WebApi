using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intranet.Application.DTOs;

namespace Intranet.Application.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default);
        Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ProductDto> CreateProductAsync(CreateUpdateProductDto productDto, CancellationToken cancellationToken = default);
        Task<bool> UpdateProductAsync(int id, CreateUpdateProductDto productDto, CancellationToken cancellationToken = default);
        Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default);
    }
}
