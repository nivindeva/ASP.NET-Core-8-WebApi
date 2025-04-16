using Intranet.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net.Mime;
using System.Text.Json;

namespace Intranet.Api.Controllers
{
    [Route("Common")] // Renamed route to avoid conflict if a "Common" controller exists
    [ApiController]
    // NOTE: [Authorize] attribute removed as requested. All endpoints are implicitly anonymous.
    // Add [AllowAnonymous] explicitly if needed for clarity, but it's redundant without a top-level [Authorize].
    [Produces(MediaTypeNames.Application.Json)] // Default produces type
    public class CommonController : ControllerBase // Inherit ControllerBase for API controllers
    {
        private readonly ICommonService _commonService;
        private readonly ILogger<CommonController> _logger;
        // IHostEnvironment _env was not used in the original logic, so removed.
        // IConfiguration _config is now primarily handled in DAL/Service where needed (e.g., connection string)

        public CommonController(ICommonService commonService, ILogger<CommonController> logger)
        {
            _commonService = commonService ?? throw new ArgumentNullException(nameof(commonService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("HelloWorld")] // Route adjusted relative to controller route
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult HelloWorld() // Return IActionResult for better consistency
        {
            _logger.LogInformation("Executing HelloWorld endpoint.");
            // Add a custom HTTP header
            HttpContext.Response.Headers.Append("Custom-Header", "Custom-Value"); // Use Append for headers
            // Note on CORS: Exposing headers like "Custom-Header" needs configuration in Program.cs
            // Example: .WithExposedHeaders("Date", "Custom-Header")

            var dateTime = System.DateTime.Now;
            var dateValue = dateTime.ToString("yyyy/MM/dd");
            // Using anonymous type with System.Text.Json serialization
            var message = JsonSerializer.Serialize(new { title = "Hello From Intranet API (Legacy Endpoint)", ServerDate = dateValue });
            // Return as ContentResult with correct content type
            return Content(message, MediaTypeNames.Application.Json);
        }

        [HttpGet("ErrorSimulate")]
        public IActionResult ErrorSimulate() // Return IActionResult
        {
            _logger.LogWarning("Executing ErrorSimulate endpoint - throwing test exception.");
            // This will be caught by the GlobalExceptionHandlerMiddleware
            throw new ArgumentException("Intranet Test API - Testing throw Error and Catching It In .NET Core 8.0");
        }

        //[HttpPost("ValidateLogin")]
        //[Consumes(MediaTypeNames.Application.Json)]
        //[ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> ValidateLogin([FromBody] JsonElement param, CancellationToken cancellationToken)
        //{
        //    _logger.LogInformation("Executing ValidateLogin endpoint.");
        //    try
        //    {
        //        // Serialize the JsonElement to pass as string, matching service expectation
        //        string json = JsonSerializer.Serialize(param);
        //        string result = await _commonService.ValidateLoginAsync(json, cancellationToken);
        //        // Return raw string result, assuming it's JSON or a specific message
        //        return Content(result, MediaTypeNames.Application.Json); // Assume result is JSON
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error in ValidateLogin endpoint.");
        //        // Let global handler manage response, or return specific ProblemDetails here
        //        return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during login validation.");
        //    }
        //}

        [HttpPost("CallSP")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)] // For parsing/SP not found errors
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Generic([FromBody] JsonElement param, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Executing Generic endpoint.");
            try
            {
                string json = JsonSerializer.Serialize(param);
                string result = await _commonService.ExecuteGenericStoredProcedureAsync(json, cancellationToken);
                return Content(result, MediaTypeNames.Application.Json); // Assume result is JSON
            }
            catch (ArgumentException ex) // Catch specific errors like missing 'FromApi'
            {
                _logger.LogWarning(ex, "Bad request in Generic endpoint.");
                return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = ex.Message, Status = StatusCodes.Status400BadRequest });
            }
            catch (InvalidOperationException ex) // Catch specific errors like SP not found
            {
                _logger.LogWarning(ex, "Invalid operation in Generic endpoint (e.g., SP not found).");
                // Return 400 or 404 depending on desired semantics for "Invalid API Call"
                return BadRequest(new ProblemDetails { Title = "Invalid API Call", Detail = ex.Message, Status = StatusCodes.Status400BadRequest });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Generic endpoint.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during the generic operation.");
            }
        }

        //// Removed [Authorize] and JWT logic - behaves same as Generic now
        //[HttpPost("GenericNT")] // Route adjusted relative to controller route
        //[Consumes(MediaTypeNames.Application.Json)]
        //[ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> GenericNT([FromBody] JsonElement param, CancellationToken cancellationToken)
        //{
        //    _logger.LogInformation("Executing GenericNT endpoint.");
        //    // Functionally identical to Generic without authorization check
        //    try
        //    {
        //        string json = JsonSerializer.Serialize(param);
        //        string result = await _commonService.ExecuteGenericStoredProcedureAsync(json, cancellationToken);
        //        return Content(result, MediaTypeNames.Application.Json); // Assume result is JSON
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        _logger.LogWarning(ex, "Bad request in GenericNT endpoint.");
        //        return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = ex.Message, Status = StatusCodes.Status400BadRequest });
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        _logger.LogWarning(ex, "Invalid operation in GenericNT endpoint (e.g., SP not found).");
        //        return BadRequest(new ProblemDetails { Title = "Invalid API Call", Detail = ex.Message, Status = StatusCodes.Status400BadRequest });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error in GenericNT endpoint.");
        //        return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during the generic operation.");
        //    }
        //}

        //// Removed [Authorize]
        //[HttpPost("ReportLoad")] // Route adjusted relative to controller route
        //[Consumes(MediaTypeNames.Application.Json)]
        //[ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> ReportLoad([FromBody] JsonElement param, CancellationToken cancellationToken)
        //{
        //    _logger.LogInformation("Executing ReportLoad endpoint.");
        //    try
        //    {
        //        string json = JsonSerializer.Serialize(param);
        //        string result = await _commonService.ExecuteReportLoadStoredProcedureAsync(json, cancellationToken);
        //        return Content(result, MediaTypeNames.Application.Json); // Assume result is JSON
        //    }
        //    catch (InvalidOperationException ex) // Catch specific errors like SP not found
        //    {
        //        _logger.LogWarning(ex, "Invalid operation in ReportLoad endpoint (e.g., SP not found).");
        //        return BadRequest(new ProblemDetails { Title = "Invalid API Call", Detail = ex.Message, Status = StatusCodes.Status400BadRequest });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error in ReportLoad endpoint.");
        //        return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during the report load operation.");
        //    }
        //}

        //// Removed [AllowAnonymous] as controller has no [Authorize]
        //// Kept original file handling logic
        //[HttpPost("UploadFile")]
        //// Expects multipart/form-data, not application/json
        //[Consumes("multipart/form-data")]
        //[ProducesResponseType(typeof(object), StatusCodes.Status200OK)] // Returns anonymous object
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> UploadFile(IFormFile file, [FromForm] string UploadedBy, [FromForm] string DocumentType_Id, CancellationToken cancellationToken)
        //{
        //    _logger.LogInformation("Executing UploadFile endpoint: User={UploadedBy}, TypeId={DocumentTypeId}, FileName={FileName}", UploadedBy, DocumentType_Id, file?.FileName);
        //    try
        //    {
        //        if (file == null || file.Length == 0)
        //        {
        //            _logger.LogWarning("UploadFile endpoint called with empty file.");
        //            return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "Empty File - Nothing To Save.", Status = StatusCodes.Status400BadRequest });
        //        }

        //        // Security Note: Using DocumentType_Id and file.FileName directly in path can be risky (Path Traversal).
        //        // Consider sanitizing inputs or using generated filenames.
        //        // Ensure the base path "uploads" is secured and configured appropriately.
        //        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", DocumentType_Id); // Store uploads in a subfolder within the app directory
        //        _logger.LogInformation("Ensuring upload directory exists: {UploadDirectory}", uploadsDir);
        //        Directory.CreateDirectory(uploadsDir); // Ensures the directory exists

        //        // Security Note: Using client-provided filename directly can lead to overwrites or invalid path characters.
        //        // Consider generating a unique filename or sanitizing file.FileName.
        //        var safeFileName = Path.GetFileName(file.FileName); // Basic sanitation
        //        var filePath = Path.Combine(uploadsDir, safeFileName);
        //        _logger.LogInformation("Saving uploaded file to: {FilePath}", filePath);

        //        await using (var stream = new FileStream(filePath, FileMode.Create))
        //        {
        //            await file.CopyToAsync(stream, cancellationToken);
        //        }
        //        _logger.LogInformation("File saved successfully. Recording upload in database.");

        //        // Call the service to record the upload in the database
        //        string dbReturn = await _commonService.RecordUploadedDocumentAsync(UploadedBy, DocumentType_Id, safeFileName, cancellationToken);

        //        _logger.LogInformation("Database record result: {DbResult}", dbReturn);
        //        // Return the raw result from the SP, wrapped in a JSON object as per original code
        //        return Ok(new { ReturnMessage = dbReturn });

        //    }
        //    catch (IOException ioEx)
        //    {
        //        _logger.LogError(ioEx, "IO Error during file upload: User={UploadedBy}, TypeId={DocumentTypeId}, FileName={FileName}", UploadedBy, DocumentType_Id, file?.FileName);
        //        return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while saving the file.");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Generic error during file upload: User={UploadedBy}, TypeId={DocumentTypeId}, FileName={FileName}", UploadedBy, DocumentType_Id, file?.FileName);
        //        // Let global handler manage response, or return specific ProblemDetails
        //        return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred during file upload.");
        //    }
        //}

        //// Removed [AllowAnonymous]
        //[HttpPost("DownloadFile")]
        //[Consumes(MediaTypeNames.Application.Json)] // Expects JSON body for parameters
        //[ProducesResponseType(StatusCodes.Status200OK)] // Returns File
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> DownloadFile([FromBody] JsonElement param, CancellationToken cancellationToken) // Added CancellationToken
        //{
        //    string documentType_Id;
        //    string fileName;
        //    // string downLoadedBy; // Not used in the file retrieval logic itself

        //    _logger.LogInformation("Executing DownloadFile endpoint.");
        //    try
        //    {
        //        // Parse JsonElement using System.Text.Json or switch to Newtonsoft if preferred for dynamic
        //        // Using Newtonsoft.Json here to easily parse dynamic structure as in original code
        //        string json = JsonSerializer.Serialize(param);
        //        dynamic data = JObject.Parse(json);

        //        documentType_Id = data.DocumentType_Id;
        //        fileName = data.FileName;
        //        // downLoadedBy = data.DownLoadedBy; // Can be logged if needed

        //        if (string.IsNullOrWhiteSpace(documentType_Id) || string.IsNullOrWhiteSpace(fileName))
        //        {
        //            _logger.LogWarning("DownloadFile called with missing DocumentType_Id or FileName.");
        //            return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "DocumentType_Id and FileName are required.", Status = StatusCodes.Status400BadRequest });
        //        }
        //        _logger.LogInformation("Attempting to download file: TypeId={DocumentTypeId}, FileName={FileName}", documentType_Id, fileName);

        //        // Construct the file path based on the provided document type and file name
        //        // Security Note: Ensure inputs are sanitized.
        //        var safeDocumentTypeId = Path.GetFileName(documentType_Id); // Basic sanitation
        //        var safeFileName = Path.GetFileName(fileName); // Basic sanitation

        //        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", safeDocumentTypeId);
        //        var filePath = Path.Combine(uploadsDir, safeFileName);
        //        _logger.LogInformation("Checking for file at path: {FilePath}", filePath);

        //        // Check if the file exists
        //        if (!System.IO.File.Exists(filePath))
        //        {
        //            _logger.LogWarning("File not found at path: {FilePath}", filePath);
        //            return NotFound(new ProblemDetails { Title = "Not Found", Detail = "File not found.", Status = StatusCodes.Status404NotFound });
        //        }

        //        // Read the file content into memory
        //        _logger.LogInformation("Reading file content from: {FilePath}", filePath);
        //        var memory = new MemoryStream();
        //        await using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)) // Allow read sharing
        //        {
        //            await stream.CopyToAsync(memory, cancellationToken);
        //        }
        //        memory.Position = 0; // Reset stream position for reading

        //        _logger.LogInformation("Returning file: {FileName}", safeFileName);
        //        // Return the file using FileStreamResult for potentially large files, or FileContentResult
        //        // Using FileStreamResult is generally better for large files as it streams directly.
        //        // return File(memory, GetContentType(filePath), safeFileName);
        //        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        //        return File(fileStream, GetContentType(filePath), safeFileName); // Return stream directly
        //    }
        //    catch (FileNotFoundException fnfEx)
        //    {
        //        _logger.LogWarning(fnfEx, "File not found during download attempt.");
        //        return NotFound(new ProblemDetails { Title = "Not Found", Detail = "File not found.", Status = StatusCodes.Status404NotFound });
        //    }
        //    catch (IOException ioEx)
        //    {
        //        _logger.LogError(ioEx, "IO Error during file download.");
        //        return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while reading the file.");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Generic error during file download.");
        //        return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred during file download.");
        //    }
        //}

        //// Helper method to determine the content type based on file extension (Keep as is)
        //private string GetContentType(string path)
        //{
        //    var types = GetMimeTypes();
        //    var ext = Path.GetExtension(path).ToLowerInvariant();
        //    return types.ContainsKey(ext) ? types[ext] : "application/octet-stream"; // Default fallback
        //}

        //// Mapping of file extensions to MIME types (Keep as is)
        //private Dictionary<string, string> GetMimeTypes()
        //{
        //    // Can be loaded from configuration or kept static
        //    return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) // Case-insensitive extensions
        //    {
        //        { ".txt", "text/plain" },
        //        { ".pdf", "application/pdf" },
        //        { ".doc", "application/vnd.ms-word" },
        //        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        //        { ".xls", "application/vnd.ms-excel" },
        //        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        //        { ".png", "image/png" },
        //        { ".jpg", "image/jpeg" },
        //        { ".jpeg", "image/jpeg" },
        //        { ".gif", "image/gif" },
        //        { ".csv", "text/csv" },
        //        { ".zip", "application/zip" },
        //        // Add more as needed
        //    };
        //}
   
    }
}