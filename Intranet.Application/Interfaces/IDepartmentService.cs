using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intranet.Application.DTOs;

namespace Intranet.Application.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync(CancellationToken cancellationToken = default);
        Task<DepartmentDto?> GetDepartmentByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<DepartmentDto> CreateDepartmentAsync(CreateUpdateDepartmentDto departmentDto, CancellationToken cancellationToken = default);
        Task<bool> UpdateDepartmentAsync(int id, CreateUpdateDepartmentDto departmentDto, CancellationToken cancellationToken = default);
        Task<bool> DeleteDepartmentAsync(int id, CancellationToken cancellationToken = default);
    }
}
