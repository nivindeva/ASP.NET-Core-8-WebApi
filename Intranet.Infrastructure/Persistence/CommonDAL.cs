using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intranet.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Intranet.Infrastructure.Persistence
{
    // NOTE: This DAL is designed to replicate the behavior of the original 'DAL.ClsDAL'
    // for the specific purpose of porting the 'CommonController' logic.
    // It directly executes provided SqlCommand objects and expects the Stored Procedure
    // itself to return the result as a single JSON string.
    public class CommonDAL : ICommonDAL
    {
        private readonly string _connectionString;
        private readonly ILogger<CommonDAL> _logger;

        public CommonDAL(IConfiguration configuration, ILogger<CommonDAL> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes a SqlCommand (expected to be a Stored Procedure call)
        /// and returns the result directly as a JSON string.
        /// Assumes the Stored Procedure returns the JSON string as the first column of the first row.
        /// </summary>
        /// <param name="command">The SqlCommand object, pre-configured with CommandText and Parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The JSON string result from the Stored Procedure, or null/empty string if no result.</returns>
        public async Task<string?> Dal_Cmd_JSONStringAsync(SqlCommand command, CancellationToken cancellationToken = default)
        {
            try
            {
                await using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);
                    command.Connection = connection; // Assign the connection to the command

                    _logger.LogInformation("Executing Stored Procedure (expecting direct JSON result): {StoredProcedureName}", command.CommandText);

                    // Use ExecuteScalarAsync as we expect the SP to return the JSON string
                    // as the single result (first column of the first row).
                    object? result = await command.ExecuteScalarAsync(cancellationToken);

                    if (result == null || result == DBNull.Value)
                    {
                        _logger.LogWarning("Stored Procedure {StoredProcedureName} returned null or DBNull via ExecuteScalarAsync.", command.CommandText);
                        // Return an empty JSON array string "[]" to mimic potential previous behavior
                        // or null if that's acceptable for the calling service/controller.
                        return "[]";
                    }
                    else
                    {
                        // Convert the scalar result directly to a string.
                        string jsonResult = Convert.ToString(result);
                        _logger.LogInformation("Successfully executed {StoredProcedureName} and received direct JSON result.", command.CommandText);
                        return jsonResult;
                    }
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Error executing command for Stored Procedure: {StoredProcedureName}", command.CommandText);
                throw; // Re-throw to be handled upstream
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic error executing command for Stored Procedure: {StoredProcedureName}", command.CommandText);
                throw;
            }
        }
    }
}
