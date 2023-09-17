namespace ProjectManagement.Application.Contracts.Class
{
    public class StudentProgressInfo
    {
        public string Id { get; set; }
        public string Forename { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public List<ModuleProgressInfo> ModuleProgress { get; set; }


    }
}
