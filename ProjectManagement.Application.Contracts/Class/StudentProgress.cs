using System.Data;

namespace ProjectManagement.Application.Contracts.Class
{
    public class StudentProgress
    {
        public string Module { get; set; }
        public DataTable Info { get; set; }
        public DataTable Mark { get; set; }
        public DataTable Attendance { get; set; }
        public DataTable Checkpoint { get; set; }
    }

    
}

