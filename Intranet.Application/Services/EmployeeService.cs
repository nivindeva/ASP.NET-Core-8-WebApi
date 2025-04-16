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
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<EmployeeService> _logger;
        private readonly IMapper _mapper; // Added

        // Constructor updated for AutoMapper
        public EmployeeService(IEmployeeRepository employeeRepository, ILogger<EmployeeService> logger, IMapper mapper)
        {
            _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper)); // Added assignment
        }

        public async Task<EmployeeDto> CreateEmployeeAsync(CreateUpdateEmployeeDto employeeDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to create a new employee: {EmployeeEmail}", employeeDto.Email);
            // Map DTO to Entity using AutoMapper
            var employee = _mapper.Map<Employee>(employeeDto); // Updated

            var addedEmployee = await _employeeRepository.AddAsync(employee, cancellationToken);

            _logger.LogInformation("Employee created successfully with ID: {EmployeeId}", addedEmployee.Id);
            // Map Entity back to DTO using AutoMapper
            return _mapper.Map<EmployeeDto>(addedEmployee); // Updated
        }

        public async Task<bool> DeleteEmployeeAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to delete employee with ID: {EmployeeId}", id);
            var result = await _employeeRepository.DeleteAsync(id, cancellationToken);
            if (result)
            {
                _logger.LogInformation("Employee with ID: {EmployeeId} deleted successfully.", id);
            }
            else
            {
                _logger.LogWarning("Employee with ID: {EmployeeId} not found for deletion.", id);
            }
            return result;
            // No mapping changes needed here
        }

        public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all employees.");
            var employees = await _employeeRepository.GetAllAsync(cancellationToken);
            // Map IEnumerable<Entity> to IEnumerable<DTO>
            return _mapper.Map<IEnumerable<EmployeeDto>>(employees); // Updated
        }

        public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching employee with ID: {EmployeeId}", id);
            var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken);
            if (employee == null)
            {
                _logger.LogWarning("Employee with ID: {EmployeeId} not found.", id);
                return null;
            }
            // Map Entity to DTO
            return _mapper.Map<EmployeeDto>(employee); // Updated
        }

        public async Task<IEnumerable<EmployeeDto>> GetEmployeesByDepartmentAsync(int departmentId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching employees for Department ID: {DepartmentId}", departmentId);
            var employees = await _employeeRepository.GetByDepartmentIdAsync(departmentId, cancellationToken);
            // Map IEnumerable<Entity> to IEnumerable<DTO>
            return _mapper.Map<IEnumerable<EmployeeDto>>(employees); // Updated
        }


        public async Task<bool> UpdateEmployeeAsync(int id, CreateUpdateEmployeeDto employeeDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to update employee with ID: {EmployeeId}", id);
            var existingEmployee = await _employeeRepository.GetByIdAsync(id, cancellationToken);
            if (existingEmployee == null)
            {
                _logger.LogWarning("Employee with ID: {EmployeeId} not found for update.", id);
                return false;
            }

            // Use AutoMapper to map updated fields from DTO onto the existing entity
            _mapper.Map(employeeDto, existingEmployee); // Updated

            var result = await _employeeRepository.UpdateAsync(existingEmployee, cancellationToken);
            if (result)
            {
                _logger.LogInformation("Employee with ID: {EmployeeId} updated successfully.", id);
            }
            else
            {
                _logger.LogError("Failed to update employee with ID: {EmployeeId}.", id);
            }
            return result;
        }
    }
}