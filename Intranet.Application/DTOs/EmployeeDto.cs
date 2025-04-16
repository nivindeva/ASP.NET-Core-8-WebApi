using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intranet.Application.DTOs
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}"; // Example of computed property
        public string Email { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        // Optional: Include Department Name if needed (requires joining in repository or separate lookup)
        // public string? DepartmentName { get; set; }
    }

    public class CreateUpdateEmployeeDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
    }
}
