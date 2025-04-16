using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intranet.Domain.Entities;

namespace Intranet.Domain.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        // Add product-specific methods here if any
    }
}
