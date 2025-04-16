using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intranet.Domain.Entities
{
    public class Employee
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int? DepartmentId { get; set; } // Nullable foreign key

        // Optional: Navigation property (requires EF Core or careful manual loading)
        // For raw SQL, we often omit navigation properties in the core entity
        // or load them separately in the Application/Infrastructure layer.
        // public Department? Department { get; set; }
    }
}
