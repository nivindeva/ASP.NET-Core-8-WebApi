using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intranet.Application.DTOs
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Location { get; set; }
    }

    // Separate DTO for creation/update if needed (e.g., without Id)
    public class CreateUpdateDepartmentDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Location { get; set; }
    }
}
