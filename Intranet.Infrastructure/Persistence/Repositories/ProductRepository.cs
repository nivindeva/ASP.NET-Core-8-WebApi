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
    public class ProductRepository : BaseRepository, IProductRepository
    {
        private readonly ILogger<ProductRepository> _logger; // Added logger

        public ProductRepository(IConfiguration configuration, ILogger<ProductRepository> logger)
            : base(configuration) // Pass config to base
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Product> AddAsync(Product entity, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                INSERT INTO Products (Name, Description, Price)
                VALUES (@Name, @Description, @Price);
                SELECT CAST(SCOPE_IDENTITY() as int)"; // Get the inserted ID

            try
            {
                await using var connection = CreateConnection();
                await connection.OpenAsync(cancellationToken);

                await using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Name", entity.Name);
                // Handle potential null description
                command.Parameters.AddWithValue("@Description", (object?)entity.Description ?? DBNull.Value);
                command.Parameters.AddWithValue("@Price", entity.Price);

                // ExecuteScalarAsync returns the first column of the first row (the new ID)
                var newId = (int?)await command.ExecuteScalarAsync(cancellationToken);

                if (!newId.HasValue)
                {
                    throw new InvalidOperationException("Failed to retrieve new Product ID after insertion.");
                }
                entity.Id = newId.Value;
                _logger.LogInformation("Product added successfully with ID {ProductId}", entity.Id);
                return entity; // Return the entity with the new ID
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Error occurred while adding product: {ProductName}", entity.Name);
                throw; // Re-throw to allow higher layers to handle (e.g., global exception handler)
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic Error occurred while adding product: {ProductName}", entity.Name);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = "DELETE FROM Products WHERE Id = @Id";
            try
            {
                await using var connection = CreateConnection();
                await connection.OpenAsync(cancellationToken);
                await using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Id", id);

                int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
                _logger.LogInformation("Delete operation executed for Product ID {ProductId}. Rows affected: {RowsAffected}", id, rowsAffected);
                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Error occurred while deleting product with ID: {ProductId}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic Error occurred while deleting product with ID: {ProductId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT Id, Name, Description, Price FROM Products";
            var products = new List<Product>();

            try
            {
                await using var connection = CreateConnection();
                await connection.OpenAsync(cancellationToken);
                await using var command = new SqlCommand(sql, connection);
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);

                while (await reader.ReadAsync(cancellationToken))
                {
                    products.Add(MapRowToProduct(reader));
                }
                _logger.LogInformation("Retrieved {ProductCount} products.", products.Count);
                return products;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Error occurred while retrieving all products.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic Error occurred while retrieving all products.");
                throw;
            }
        }


        public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT Id, Name, Description, Price FROM Products WHERE Id = @Id";
            try
            {
                await using var connection = CreateConnection();
                await connection.OpenAsync(cancellationToken);
                await using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Id", id);

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);

                if (await reader.ReadAsync(cancellationToken))
                {
                    _logger.LogInformation("Product found with ID {ProductId}", id);
                    return MapRowToProduct(reader);
                }
                _logger.LogWarning("Product not found with ID {ProductId}", id);
                return null; // Not found
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Error occurred while retrieving product with ID: {ProductId}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic Error occurred while retrieving product with ID: {ProductId}", id);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Product entity, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                UPDATE Products SET
                    Name = @Name,
                    Description = @Description,
                    Price = @Price
                WHERE Id = @Id";
            try
            {
                await using var connection = CreateConnection();
                await connection.OpenAsync(cancellationToken);
                await using var command = new SqlCommand(sql, connection);

                command.Parameters.AddWithValue("@Id", entity.Id);
                command.Parameters.AddWithValue("@Name", entity.Name);
                command.Parameters.AddWithValue("@Description", (object?)entity.Description ?? DBNull.Value);
                command.Parameters.AddWithValue("@Price", entity.Price);

                int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
                _logger.LogInformation("Update operation executed for Product ID {ProductId}. Rows affected: {RowsAffected}", entity.Id, rowsAffected);
                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Error occurred while updating product with ID: {ProductId}", entity.Id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic Error occurred while updating product with ID: {ProductId}", entity.Id);
                throw;
            }
        }

        // Helper method to map a SqlDataReader row to a Product object
        private static Product MapRowToProduct(SqlDataReader reader)
        {
            return new Product
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")), // Use GetOrdinal for robustness
                Name = reader.GetString(reader.GetOrdinal("Name")),
                // Handle potential DBNull for nullable fields
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price"))
            };
        }
    }
}
