using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intranet.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Intranet.Application.Services
{
    // NOTE: This service adapts the logic from the original 'StageFourBLL.ClsCommon'.
    // It directly interacts with a specific DAL implementation (`ICommonDAL`)
    // and bypasses the standard repository/DTO pattern for these specific actions
    // as per the requirement to port the existing code structure.
    public class CommonService : ICommonService
    {
        private readonly ICommonDAL _commonDAL;
        private readonly ILogger<CommonService> _logger;

        public CommonService(ICommonDAL commonDAL, ILogger<CommonService> logger)
        {
            _commonDAL = commonDAL ?? throw new ArgumentNullException(nameof(commonDAL));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Adapted from Bll_ValidateLogin - Removed JWT Generation
        public async Task<string> ValidateLoginAsync(string paramAsJsonString, CancellationToken cancellationToken = default)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "P_VALIDATELOGIN"; // Stored Procedure Name
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@param", paramAsJsonString);

            try
            {
                string? retFromDB = await _commonDAL.Dal_Cmd_JSONStringAsync(cmd, cancellationToken);

                if (!string.IsNullOrEmpty(retFromDB))
                {
                    // Original code modified the JSON to add a token.
                    // Since JWT is removed, we just return the result from the SP.
                    _logger.LogInformation("Login validation successful for parameters: {Parameters}", paramAsJsonString);
                    return retFromDB;
                }
                else
                {
                    _logger.LogWarning("Login validation returned no data for parameters: {Parameters}", paramAsJsonString);
                    // Return an empty array or specific message based on expected client handling
                    return "[]"; // Or "No Data Found" or throw new UnauthorizedAccessException(...)
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login validation for parameters: {Parameters}", paramAsJsonString);
                throw; // Let the controller/middleware handle it
            }
        }

        // Adapted from Bll_Generic
        public async Task<string> ExecuteGenericStoredProcedureAsync(string paramAsJsonString, CancellationToken cancellationToken = default)
        {
            string spName;
            try
            {
                // Using Newtonsoft.Json here to match original code's parsing behavior
                dynamic data = JObject.Parse(paramAsJsonString);
                string? apiName = data.FromApi; // Assuming the JSON contains a "FromApi" field for the SP name part

                if (string.IsNullOrWhiteSpace(apiName))
                {
                    throw new ArgumentException("Missing 'FromApi' field in the request JSON to determine the stored procedure.");
                }
                spName = "P_" + apiName.ToUpper();
                _logger.LogInformation("Attempting to execute generic stored procedure: {StoredProcedureName}", spName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing parameters for generic stored procedure call. Input JSON: {InputJson}", paramAsJsonString);
                throw new ArgumentException("Invalid JSON format or missing 'FromApi' field.", ex);
            }


            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = spName;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@param", paramAsJsonString); // Pass the full JSON string as parameter

            try
            {
                string? retFromDB = await _commonDAL.Dal_Cmd_JSONStringAsync(cmd, cancellationToken);
                return retFromDB ?? "[]"; // Return empty array string if null
            }
            catch (SqlException ex) when (ex.Message.ToLower().Contains("could not find stored procedure"))
            {
                _logger.LogWarning(ex, "Invalid API Call - Stored procedure not found: {StoredProcedureName}", spName);
                // Throw a more specific/user-friendly exception if desired
                throw new InvalidOperationException($"Invalid API Call: The target '{spName}' was not found.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing generic stored procedure {StoredProcedureName} with parameters: {Parameters}", spName, paramAsJsonString);
                throw;
            }
        }

        // Adapted from Bll_UploadedDocument
        public async Task<string> RecordUploadedDocumentAsync(string uploadedBy, string refDocumentTypeId, string documentName, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Recording uploaded document: User={UploadedBy}, TypeId={RefDocumentTypeId}, Name={DocumentName}", uploadedBy, refDocumentTypeId, documentName);
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "P_UPLOAD"; // Stored Procedure Name
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@UploadedBy", uploadedBy);
            cmd.Parameters.AddWithValue("@RefDocumentType_Id", refDocumentTypeId);
            cmd.Parameters.AddWithValue("@DocumentName", documentName);

            try
            {
                string? retFromDB = await _commonDAL.Dal_Cmd_JSONStringAsync(cmd, cancellationToken);
                return retFromDB ?? "[]"; // Return empty array string if null
            }
            catch (SqlException ex) when (ex.Message.ToLower().Contains("could not find stored procedure"))
            {
                _logger.LogWarning(ex, "Invalid API Call - Stored procedure not found: P_UPLOAD");
                throw new InvalidOperationException("Invalid API Call: The target 'P_UPLOAD' was not found.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing P_UPLOAD for User={UploadedBy}, TypeId={RefDocumentTypeId}, Name={DocumentName}", uploadedBy, refDocumentTypeId, documentName);
                throw;
            }
        }

        // Adapted from Bll_ReportLoad
        public async Task<string> ExecuteReportLoadStoredProcedureAsync(string paramAsJsonString, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Executing report load stored procedure.");
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "P_REPORTLOAD"; // Stored Procedure Name
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@param", paramAsJsonString);

            try
            {
                string? retFromDB = await _commonDAL.Dal_Cmd_JSONStringAsync(cmd, cancellationToken);
                return retFromDB ?? "[]"; // Return empty array string if null
            }
            catch (SqlException ex) when (ex.Message.ToLower().Contains("could not find stored procedure"))
            {
                _logger.LogWarning(ex, "Invalid API Call - Stored procedure not found: P_REPORTLOAD");
                throw new InvalidOperationException("Invalid API Call: The target 'P_REPORTLOAD' was not found.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing P_REPORTLOAD with parameters: {Parameters}", paramAsJsonString);
                throw;
            }
        }
    }
}
