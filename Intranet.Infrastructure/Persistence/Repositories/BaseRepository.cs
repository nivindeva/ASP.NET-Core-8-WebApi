using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Intranet.Infrastructure.Persistence.Repositories
{
    public abstract class BaseRepository
    {
        protected readonly string _connectionString;
        protected readonly IConfiguration _configuration;

        protected BaseRepository(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            // Recommended: Get connection string from configuration
            _connectionString = _configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        protected SqlConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
