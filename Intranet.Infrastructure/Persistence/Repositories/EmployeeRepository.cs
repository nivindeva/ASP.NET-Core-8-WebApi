using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intranet.Domain.Entities;
using Intranet.Domain.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Intranet.Infrastructure.Persistence.Repositories
{
    public class EmployeeRepository : BaseRepository, IEmployeeRepository
    {
        private readonly ILogger<EmployeeRepository> _logger;

        public EmployeeRepository(IConfiguration configuration, ILogger<EmployeeRepository> logger)
            : base(configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Employee> AddAsync(Employee entity, CancellationToken cancellationToken = default)
        {
            // Check for uniqueness constraint on Email before insert if desired,
            // or let the database handle it and catch the SqlException.
            // For simplicity, we let the DB handle it here.
            const string sql = @"
                INSERT INTO Employees (FirstName, LastName, Email, DepartmentId)
                VALUES (@FirstName, @LastName, @Email, @DepartmentId);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            try
            {
                await using var connection = CreateConnection();
                await connection.OpenAsync(cancellationToken);

                await using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@FirstName", entity.FirstName);
                command.Parameters.AddWithValue("@LastName", entity.LastName);
                command.Parameters.AddWithValue("@Email", entity.Email);
                command.Parameters.AddWithValue("@DepartmentId", (object?)entity.DepartmentId ?? DBNull.Value); // Handle nullable FK

                var newId = (int?)await command.ExecuteScalarAsync(cancellationToken);

                if (!newId.HasValue)
                {
                    throw new InvalidOperationException("Failed to retrieve new Employee ID after insertion.");
                }
                entity.Id = newId.Value;
                _logger.LogInformation("Employee added successfully with ID {EmployeeId}", entity.Id);
                return entity;
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627) // Unique constraint violation
            {
                _logger.LogWarning(ex, "SQL Error: Attempted to add employee with duplicate email: {Email}", entity.Email);
                // Throw a more specific exception or return a specific result if needed
                throw new InvalidOperationException($"An employee with the email '{entity.Email}' already exists.", ex);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Error occurred while adding employee: {EmployeeName}", $"{entity.FirstName} {entity.LastName}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic Error occurred while adding employee: {EmployeeName}", $"{entity.FirstName} {entity.LastName}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = "DELETE FROM Employees WHERE Id = @Id";
            try
            {
                await using var connection = CreateConnection();
                await connection.OpenAsync(cancellationToken);
                await using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Id", id);

                int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
                _logger.LogInformation("Delete operation executed for Employee ID {EmployeeId}. Rows affected: {RowsAffected}", id, rowsAffected);
                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Error occurred while deleting employee with ID: {EmployeeId}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic Error occurred while deleting employee with ID: {EmployeeId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Employee>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT Id, FirstName, LastName, Email, DepartmentId FROM Employees";
            var employees = new List<Employee>();

            try
            {
                await using var connection = CreateConnection();
                await connection.OpenAsync(cancellationToken);
                await using var command = new SqlCommand(sql, connection);
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);

                while (await reader.ReadAsync(cancellationToken))
                {
                    employees.Add(MapRowToEmployee(reader));
                }
                _logger.LogInformation("Retrieved {EmployeeCount} employees.", employees.Count);
                return employees;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Error occurred while retrieving all employees.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic Error occurred while retrieving all employees.");
                throw;
            }
        }

        public async Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT Id, FirstName, LastName, Email, DepartmentId FROM Employees WHERE Id = @Id";
            try
            {
                await using var connection = CreateConnection();
                await connection.OpenAsync(cancellationToken);
                await using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Id", id);

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);

                if (await reader.ReadAsync(cancellationToken))
                {
                    _logger.LogInformation("Employee found with ID {EmployeeId}", id);
                    return MapRowToEmployee(reader);
                }
                _logger.LogWarning("Employee not found with ID {EmployeeId}", id);
                return null; // Not found
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Error occurred while retrieving employee with ID: {EmployeeId}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic Error occurred while retrieving employee with ID: {EmployeeId}", id);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Employee entity, CancellationToken cancellationToken = default)
        {
            // Consider checking if the email is being changed to one that already exists (excluding the current employee's record)
            const string sql = @"
                UPDATE Employees SET
                    FirstName = @FirstName,
                    LastName = @LastName,
                    Email = @Email,
                    DepartmentId = @DepartmentId
                WHERE Id = @Id";
            try
            {
                await using var connection = CreateConnection();
                await connection.OpenAsync(cancellationToken);
                await using var command = new SqlCommand(sql, connection);

                command.Parameters.AddWithValue("@Id", entity.Id);
                command.Parameters.AddWithValue("@FirstName", entity.FirstName);
                command.Parameters.AddWithValue("@LastName", entity.LastName);
                command.Parameters.AddWithValue("@Email", entity.Email);
                command.Parameters.AddWithValue("@DepartmentId", (object?)entity.DepartmentId ?? DBNull.Value);

                int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
                _logger.LogInformation("Update operation executed for Employee ID {EmployeeId}. Rows affected: {RowsAffected}", entity.Id, rowsAffected);
                return rowsAffected > 0;
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627) // Unique constraint violation
            {
                _logger.LogWarning(ex, "SQL Error: Attempted to update employee ID {EmployeeId} with duplicate email: {Email}", entity.Id, entity.Email);
                throw new InvalidOperationException($"Cannot update employee. An employee with the email '{entity.Email}' already exists.", ex);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Error occurred while updating employee with ID: {EmployeeId}", entity.Id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic Error occurred while updating employee with ID: {EmployeeId}", entity.Id);
                throw;
            }
        }

        // --- Specific IEmployeeRepository Methods ---

        public async Task<IEnumerable<Employee>> GetByDepartmentIdAsync(int departmentId, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT Id, FirstName, LastName, Email, DepartmentId FROM Employees WHERE DepartmentId = @DepartmentId";
            var employees = new List<Employee>();

            try
            {
                await using var connection = CreateConnection();
                await connection.OpenAsync(cancellationToken);
                await using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@DepartmentId", departmentId);

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);

                while (await reader.ReadAsync(cancellationToken))
                {
                    employees.Add(MapRowToEmployee(reader));
                }
                _logger.LogInformation("Retrieved {EmployeeCount} employees for Department ID {DepartmentId}.", employees.Count, departmentId);
                return employees;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Error occurred while retrieving employees for Department ID: {DepartmentId}", departmentId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic Error occurred while retrieving employees for Department ID: {DepartmentId}", departmentId);
                throw;
            }
        }

        public async Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT Id, FirstName, LastName, Email, DepartmentId FROM Employees WHERE Email = @Email";
            try
            {
                await using var connection = CreateConnection();
                await connection.OpenAsync(cancellationToken);
                await using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Email", email);

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);

                if (await reader.ReadAsync(cancellationToken))
                {
                    _logger.LogInformation("Employee found with email {Email}", email);
                    return MapRowToEmployee(reader);
                }
                _logger.LogWarning("Employee not found with email {Email}", email);
                return null; // Not found
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Error occurred while retrieving employee with email: {Email}", email);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic Error occurred while retrieving employee with email: {Email}", email);
                throw;
            }
        }


        // Helper method to map a SqlDataReader row to an Employee object
        private static Employee MapRowToEmployee(SqlDataReader reader)
        {
            return new Employee
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                // Handle nullable DepartmentId carefully
                DepartmentId = reader.IsDBNull(reader.GetOrdinal("DepartmentId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("DepartmentId"))
            };
        }
    }
}
