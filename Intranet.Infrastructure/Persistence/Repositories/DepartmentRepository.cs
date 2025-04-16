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
    public class DepartmentRepository : BaseRepository, IDepartmentRepository
    {
        private readonly ILogger<DepartmentRepository> _logger;

        public DepartmentRepository(IConfiguration configuration, ILogger<DepartmentRepository> logger)
            : base(configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Department> AddAsync(Department entity, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                INSERT INTO Departments (Name, Location)
                VALUES (@Name, @Location);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            try
            {
                await using var connection = CreateConnection();
                await connection.OpenAsync(cancellationToken);

                await using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Name", entity.Name);
                command.Parameters.AddWithValue("@Location", (object?)entity.Location ?? DBNull.Value);

                var newId = (int?)await command.ExecuteScalarAsync(cancellationToken);

                if (!newId.HasValue)
                {
                    throw new InvalidOperationException("Failed to retrieve new Department ID after insertion.");
                }
                entity.Id = newId.Value;
                _logger.LogInformation("Department added successfully with ID {DepartmentId}", entity.Id);
                return entity;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Error occurred while adding department: {DepartmentName}", entity.Name);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic Error occurred while adding department: {DepartmentName}", entity.Name);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = "DELETE FROM Departments WHERE Id = @Id";
            try
            {
                await using var connection = CreateConnection();
                await connection.OpenAsync(cancellationToken);
                await using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Id", id);

                int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
                _logger.LogInformation("Delete operation executed for Department ID {DepartmentId}. Rows affected: {RowsAffected}", id, rowsAffected);
                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                // Check for specific FK constraint violation error if needed (e.g., Error Number 547)
                _logger.LogError(ex, "SQL Error occurred while deleting department with ID: {DepartmentId}", id);
                throw; // Consider wrapping in a custom exception if FK violation needs specific handling
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic Error occurred while deleting department with ID: {DepartmentId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Department>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT Id, Name, Location FROM Departments";
            var departments = new List<Department>();

            try
            {
                await using var connection = CreateConnection();
                await connection.OpenAsync(cancellationToken);
                await using var command = new SqlCommand(sql, connection);
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);

                while (await reader.ReadAsync(cancellationToken))
                {
                    departments.Add(MapRowToDepartment(reader));
                }
                _logger.LogInformation("Retrieved {DepartmentCount} departments.", departments.Count);
                return departments;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Error occurred while retrieving all departments.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic Error occurred while retrieving all departments.");
                throw;
            }
        }

        public async Task<Department?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT Id, Name, Location FROM Departments WHERE Id = @Id";
            try
            {
                await using var connection = CreateConnection();
                await connection.OpenAsync(cancellationToken);
                await using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Id", id);

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);

                if (await reader.ReadAsync(cancellationToken))
                {
                    _logger.LogInformation("Department found with ID {DepartmentId}", id);
                    return MapRowToDepartment(reader);
                }
                _logger.LogWarning("Department not found with ID {DepartmentId}", id);
                return null; // Not found
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Error occurred while retrieving department with ID: {DepartmentId}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic Error occurred while retrieving department with ID: {DepartmentId}", id);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Department entity, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                UPDATE Departments SET
                    Name = @Name,
                    Location = @Location
                WHERE Id = @Id";
            try
            {
                await using var connection = CreateConnection();
                await connection.OpenAsync(cancellationToken);
                await using var command = new SqlCommand(sql, connection);

                command.Parameters.AddWithValue("@Id", entity.Id);
                command.Parameters.AddWithValue("@Name", entity.Name);
                command.Parameters.AddWithValue("@Location", (object?)entity.Location ?? DBNull.Value);

                int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
                _logger.LogInformation("Update operation executed for Department ID {DepartmentId}. Rows affected: {RowsAffected}", entity.Id, rowsAffected);
                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Error occurred while updating department with ID: {DepartmentId}", entity.Id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic Error occurred while updating department with ID: {DepartmentId}", entity.Id);
                throw;
            }
        }

        // Helper method to map a SqlDataReader row to a Department object
        private static Department MapRowToDepartment(SqlDataReader reader)
        {
            return new Department
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Location = reader.IsDBNull(reader.GetOrdinal("Location")) ? null : reader.GetString(reader.GetOrdinal("Location"))
            };
        }
    }
}
