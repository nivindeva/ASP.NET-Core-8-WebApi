using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intranet.Application.DTOs;

namespace Intranet.Application.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync(CancellationToken cancellationToken = default);
        Task<EmployeeDto?> GetEmployeeByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<EmployeeDto>> GetEmployeesByDepartmentAsync(int departmentId, CancellationToken cancellationToken = default);
        Task<EmployeeDto> CreateEmployeeAsync(CreateUpdateEmployeeDto employeeDto, CancellationToken cancellationToken = default);
        Task<bool> UpdateEmployeeAsync(int id, CreateUpdateEmployeeDto employeeDto, CancellationToken cancellationToken = default);
        Task<bool> DeleteEmployeeAsync(int id, CancellationToken cancellationToken = default);
    }
}
