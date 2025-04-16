using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intranet.Domain.Entities;

namespace Intranet.Domain.Interfaces
{
    // Inherit from generic or define specific methods if needed
    public interface IDepartmentRepository : IGenericRepository<Department>
    {
        // Add department-specific methods here if any
        // Example: Task<Department?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    }
}
