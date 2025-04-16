using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intranet.Application.Interfaces
{
    // Mirrors the public methods of the original ClsCommon, adapted for async and DI
    public interface ICommonService
    {
        // Removed IConfiguration dependency as JWT is removed
        Task<string> ValidateLoginAsync(string paramAsJsonString, CancellationToken cancellationToken = default);
        Task<string> ExecuteGenericStoredProcedureAsync(string paramAsJsonString, CancellationToken cancellationToken = default);
        Task<string> RecordUploadedDocumentAsync(string uploadedBy, string refDocumentTypeId, string documentName, CancellationToken cancellationToken = default);
        Task<string> ExecuteReportLoadStoredProcedureAsync(string paramAsJsonString, CancellationToken cancellationToken = default);
    }
}
