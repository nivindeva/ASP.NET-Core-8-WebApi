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
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly ILogger<DepartmentService> _logger;
        private readonly IMapper _mapper; // Added

        // Constructor updated for AutoMapper
        public DepartmentService(IDepartmentRepository departmentRepository, ILogger<DepartmentService> logger, IMapper mapper)
        {
            _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper)); // Added assignment
        }

        public async Task<DepartmentDto> CreateDepartmentAsync(CreateUpdateDepartmentDto departmentDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to create a new department: {DepartmentName}", departmentDto.Name);
            // Map DTO to Entity using AutoMapper
            var department = _mapper.Map<Department>(departmentDto); // Updated

            var addedDepartment = await _departmentRepository.AddAsync(department, cancellationToken);

            _logger.LogInformation("Department created successfully with ID: {DepartmentId}", addedDepartment.Id);
            // Map Entity back to DTO using AutoMapper
            return _mapper.Map<DepartmentDto>(addedDepartment); // Updated
        }

        public async Task<bool> DeleteDepartmentAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to delete department with ID: {DepartmentId}", id);
            var result = await _departmentRepository.DeleteAsync(id, cancellationToken);
            if (result)
            {
                _logger.LogInformation("Department with ID: {DepartmentId} deleted successfully.", id);
            }
            else
            {
                _logger.LogWarning("Department with ID: {DepartmentId} not found for deletion.", id);
            }
            return result;
            // No mapping changes needed here
        }

        public async Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all departments.");
            var departments = await _departmentRepository.GetAllAsync(cancellationToken);
            // Map IEnumerable<Entity> to IEnumerable<DTO>
            return _mapper.Map<IEnumerable<DepartmentDto>>(departments); // Updated
        }

        public async Task<DepartmentDto?> GetDepartmentByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching department with ID: {DepartmentId}", id);
            var department = await _departmentRepository.GetByIdAsync(id, cancellationToken);
            if (department == null)
            {
                _logger.LogWarning("Department with ID: {DepartmentId} not found.", id);
                return null;
            }
            // Map Entity to DTO
            return _mapper.Map<DepartmentDto>(department); // Updated
        }

        public async Task<bool> UpdateDepartmentAsync(int id, CreateUpdateDepartmentDto departmentDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to update department with ID: {DepartmentId}", id);
            var existingDepartment = await _departmentRepository.GetByIdAsync(id, cancellationToken);
            if (existingDepartment == null)
            {
                _logger.LogWarning("Department with ID: {DepartmentId} not found for update.", id);
                return false;
            }

            // Use AutoMapper to map updated fields from DTO onto the existing entity
            _mapper.Map(departmentDto, existingDepartment); // Updated

            var result = await _departmentRepository.UpdateAsync(existingDepartment, cancellationToken);
            if (result)
            {
                _logger.LogInformation("Department with ID: {DepartmentId} updated successfully.", id);
            }
            else
            {
                _logger.LogError("Failed to update department with ID: {DepartmentId}.", id);
            }
            return result;
        }
    }
}