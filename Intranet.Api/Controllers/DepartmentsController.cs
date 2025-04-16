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
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;
        private readonly ILogger<DepartmentsController> _logger;
        // Optional: Inject IEmployeeService if you add an endpoint like GetEmployeesByDepartment here
        // private readonly IEmployeeService _employeeService;

        public DepartmentsController(IDepartmentService departmentService, ILogger<DepartmentsController> logger /*, IEmployeeService employeeService = null */)
        {
            _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            // _employeeService = employeeService;
        }

        /// <summary>
        /// Gets all departments.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of departments.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DepartmentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            _logger.LogInformation("API endpoint called: GetAll Departments");
            var departments = await _departmentService.GetAllDepartmentsAsync(cancellationToken);
            return Ok(departments);
        }

        /// <summary>
        /// Gets a specific department by ID.
        /// </summary>
        /// <param name="id">The ID of the department.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The requested department.</returns>
        [HttpGet("{id:int}", Name = "GetDepartmentById")] // Name needed for CreatedAtAction
        [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("API endpoint called: GetDepartmentById with ID {DepartmentId}", id);
            var department = await _departmentService.GetDepartmentByIdAsync(id, cancellationToken);
            if (department == null)
            {
                _logger.LogWarning("Department not found for ID {DepartmentId}", id);
                return NotFound(); // Standard 404
            }
            return Ok(department);
        }

        /// <summary>
        /// Creates a new department.
        /// </summary>
        /// <param name="departmentDto">The department data transfer object.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created department.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateUpdateDepartmentDto departmentDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for creating department.");
                return BadRequest(ModelState);
            }

            _logger.LogInformation("API endpoint called: Create Department with name {DepartmentName}", departmentDto.Name);
            try
            {
                var createdDepartment = await _departmentService.CreateDepartmentAsync(departmentDto, cancellationToken);
                // Return 201 Created with Location header pointing to the new resource
                return CreatedAtAction(nameof(GetById), new { id = createdDepartment.Id }, createdDepartment);
            }
            catch (InvalidOperationException ex) // Example: Catch specific exceptions from service (like duplicate name)
            {
                _logger.LogWarning("Failed to create department: {ErrorMessage}", ex.Message);
                // Return a more specific error if needed, e.g., Conflict (409) for duplicates
                return BadRequest(new ProblemDetails { Title = "Creation Failed", Detail = ex.Message, Status = StatusCodes.Status400BadRequest });
            }
            // Let the global handler catch other exceptions (DB errors, etc.)
        }

        /// <summary>
        /// Updates an existing department.
        /// </summary>
        /// <param name="id">The ID of the department to update.</param>
        /// <param name="departmentDto">The updated department data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content if successful.</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateUpdateDepartmentDto departmentDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for updating department ID {DepartmentId}.", id);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("API endpoint called: Update Department with ID {DepartmentId}", id);
            try
            {
                var success = await _departmentService.UpdateDepartmentAsync(id, departmentDto, cancellationToken);
                if (!success)
                {
                    // Could be NotFound or potentially a concurrency issue handled in the service/repo
                    _logger.LogWarning("Department not found or failed to update for ID {DepartmentId}", id);
                    return NotFound(); // Assuming failure means not found
                }
                return NoContent(); // Standard response for successful PUT
            }
            catch (InvalidOperationException ex) // Example: Catch specific exceptions from service (like duplicate name on update)
            {
                _logger.LogWarning("Failed to update department {DepartmentId}: {ErrorMessage}", id, ex.Message);
                return BadRequest(new ProblemDetails { Title = "Update Failed", Detail = ex.Message, Status = StatusCodes.Status400BadRequest });
            }
            // Let the global handler catch other exceptions
        }

        /// <summary>
        /// Deletes a department by ID.
        /// </summary>
        /// <param name="id">The ID of the department to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Added for potential FK issues if not handled gracefully
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("API endpoint called: Delete Department with ID {DepartmentId}", id);
            try
            {
                var success = await _departmentService.DeleteDepartmentAsync(id, cancellationToken);
                if (!success)
                {
                    _logger.LogWarning("Department not found for deletion with ID {DepartmentId}", id);
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex) // Catch potential FK constraint violations if the DB throws them
            {
                // The global handler will catch this, but you could add specific logging or return types
                _logger.LogError(ex, "Error occurred during deletion of Department ID {DepartmentId}", id);
                // Depending on requirements, you might return BadRequest or Conflict
                // For now, let the global handler return 500 or as configured
                throw;
            }
        }

        // --- Optional: Example of getting related data ---
        /*
        /// <summary>
        /// Gets all employees for a specific department.
        /// </summary>
        /// <param name="id">The ID of the department.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of employees in the department.</returns>
        [HttpGet("{id:int}/employees")]
        [ProducesResponseType(typeof(IEnumerable<EmployeeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)] // If department itself not found
        public async Task<IActionResult> GetEmployeesByDepartment(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("API endpoint called: GetEmployeesByDepartment for Department ID {DepartmentId}", id);

            // First check if department exists (optional, depends on desired behavior)
            var department = await _departmentService.GetDepartmentByIdAsync(id, cancellationToken);
            if (department == null)
            {
                _logger.LogWarning("Department not found when fetching employees for Department ID {DepartmentId}", id);
                return NotFound($"Department with ID {id} not found.");
            }

            // Requires _employeeService to be injected
            if (_employeeService == null) {
                 _logger.LogError("EmployeeService not injected. Cannot call GetEmployeesByDepartmentAsync.");
                 return StatusCode(StatusCodes.Status501NotImplemented, "Employee lookup functionality not available via this endpoint.");
            }

            var employees = await _employeeService.GetEmployeesByDepartmentAsync(id, cancellationToken);
            return Ok(employees);
        }
        */
    }
}
