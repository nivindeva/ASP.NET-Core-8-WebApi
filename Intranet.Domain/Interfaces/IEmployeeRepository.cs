using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intranet.Domain.Entities;

namespace Intranet.Domain.Interfaces
{
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
        Task<IEnumerable<Employee>> GetByDepartmentIdAsync(int departmentId, CancellationToken cancellationToken = default);
        Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    }
}
