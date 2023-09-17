using System.Data;

namespace ProjectManagement.Application.Contracts.Class
{
    public class ModuleProgressInfo
    {
        public string Name { get; set; }
        public DataRow Mark { get; set; }
        public DataRow Attendance { get; set; }
        public DataRow Checkpoint { get; set; }
    }
}
