using Intranet.Application.DTOs;
using Intranet.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Intranet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(IEmployeeService employeeService, ILogger<EmployeesController> logger)
        {
            _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all employees. Optional: Add filtering by department later.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of employees.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EmployeeDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            // Future enhancement: Add optional [FromQuery] int? departmentId parameter
            // and call a different service method if provided.
            _logger.LogInformation("API endpoint called: GetAll Employees");
            var employees = await _employeeService.GetAllEmployeesAsync(cancellationToken);
            return Ok(employees);
        }

        /// <summary>
        /// Gets a specific employee by ID.
        /// </summary>
        /// <param name="id">The ID of the employee.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The requested employee.</returns>
        [HttpGet("{id:int}", Name = "GetEmployeeById")] // Name needed for CreatedAtAction
        [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("API endpoint called: GetEmployeeById with ID {EmployeeId}", id);
            var employee = await _employeeService.GetEmployeeByIdAsync(id, cancellationToken);
            if (employee == null)
            {
                _logger.LogWarning("Employee not found for ID {EmployeeId}", id);
                return NotFound();
            }
            return Ok(employee);
        }

        /// <summary>
        /// Gets all employees for a specific department.
        /// </summary>
        /// <param name="departmentId">The ID of the department.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of employees in the specified department.</returns>
        [HttpGet("bydepartment/{departmentId:int}")]
        [ProducesResponseType(typeof(IEnumerable<EmployeeDto>), StatusCodes.Status200OK)]
        // Optional: Add 404 if you want to check if the department itself exists first
        public async Task<IActionResult> GetByDepartment(int departmentId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("API endpoint called: GetEmployeesByDepartment for Department ID {DepartmentId}", departmentId);
            // Consider adding check if Department ID exists if required by business logic
            var employees = await _employeeService.GetEmployeesByDepartmentAsync(departmentId, cancellationToken);
            return Ok(employees); // Returns empty list if department has no employees or doesn't exist (based on current service impl)
        }

        /// <summary>
        /// Creates a new employee.
        /// </summary>
        /// <param name="employeeDto">The employee data transfer object.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created employee.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)] // For validation and specific errors like duplicates
        public async Task<IActionResult> Create([FromBody] CreateUpdateEmployeeDto employeeDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for creating employee.");
                return BadRequest(ModelState);
            }

            _logger.LogInformation("API endpoint called: Create Employee with email {EmployeeEmail}", employeeDto.Email);
            try
            {
                // Optional: Check if DepartmentId provided in DTO actually exists
                // if(employeeDto.DepartmentId.HasValue) { /* ... call department service ... */ }

                var createdEmployee = await _employeeService.CreateEmployeeAsync(employeeDto, cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id = createdEmployee.Id }, createdEmployee);
            }
            catch (InvalidOperationException ex) // Catch specific errors like duplicate email from the service/repo layer
            {
                _logger.LogWarning("Failed to create employee: {ErrorMessage}", ex.Message);
                // Return 400 Bad Request or 409 Conflict depending on the error type
                return BadRequest(new ProblemDetails { Title = "Creation Failed", Detail = ex.Message, Status = StatusCodes.Status400BadRequest });
            }
            // Let global handler manage other exceptions
        }

        /// <summary>
        /// Updates an existing employee.
        /// </summary>
        /// <param name="id">The ID of the employee to update.</param>
        /// <param name="employeeDto">The updated employee data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content if successful.</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)] // For validation and specific errors like duplicates
        public async Task<IActionResult> Update(int id, [FromBody] CreateUpdateEmployeeDto employeeDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for updating employee ID {EmployeeId}.", id);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("API endpoint called: Update Employee with ID {EmployeeId}", id);
            try
            {
                // Optional: Check if DepartmentId provided in DTO actually exists
                // if(employeeDto.DepartmentId.HasValue) { /* ... call department service ... */ }

                var success = await _employeeService.UpdateEmployeeAsync(id, employeeDto, cancellationToken);
                if (!success)
                {
                    _logger.LogWarning("Employee not found or failed to update for ID {EmployeeId}", id);
                    return NotFound(); // Assuming failure means not found
                }
                return NoContent();
            }
            catch (InvalidOperationException ex) // Catch specific errors like duplicate email from the service/repo layer
            {
                _logger.LogWarning("Failed to update employee {EmployeeId}: {ErrorMessage}", id, ex.Message);
                return BadRequest(new ProblemDetails { Title = "Update Failed", Detail = ex.Message, Status = StatusCodes.Status400BadRequest });
            }
            // Let global handler manage other exceptions
        }

        /// <summary>
        /// Deletes an employee by ID.
        /// </summary>
        /// <param name="id">The ID of the employee to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("API endpoint called: Delete Employee with ID {EmployeeId}", id);
            var success = await _employeeService.DeleteEmployeeAsync(id, cancellationToken);
            if (!success)
            {
                _logger.LogWarning("Employee not found for deletion with ID {EmployeeId}", id);
                return NotFound();
            }
            return NoContent();
            // No specific exceptions caught here, assuming delete is straightforward or FK constraints handle related data if applicable
            // Let global handler manage DB errors etc.
        }
    }
}
