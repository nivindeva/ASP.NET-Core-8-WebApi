using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Intranet.Application.Interfaces
{
    public interface ICommonDAL
    {
        // Returns result as a raw JSON string, mimicking original DAL behavior
        Task<string?> Dal_Cmd_JSONStringAsync(SqlCommand command, CancellationToken cancellationToken = default);
    }
}
