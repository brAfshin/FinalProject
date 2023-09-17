namespace ProjectManagement.Application.Contracts.Class
{
    public class CreateReport
    {
        public List<string> Info { get; set; }
        public List<string> Mark { get; set; }
        public List<string> Attendance { get; set; }
        public List<string> Checkpoint { get; set; }
        public string Path { get; set; }
        public string Module { get; set; }
    }
}
