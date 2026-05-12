using System;
using System.Collections.Generic;
using System.Text;

namespace BusTracking.Common.DTOs.Student
{
    public class UpdateStudentExtDto
    {
        public string FullName { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string? Standard { get; set; }
        public int? BusId { get; set; }
        public int? StopId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
