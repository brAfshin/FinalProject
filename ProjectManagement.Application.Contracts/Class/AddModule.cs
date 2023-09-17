namespace ProjectManagement.Application.Contracts.Class
{
    public class AddModule
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string SourcePath { get; set; }
        public string dbPath { get; set; }
        public string StartingRow { get; set; }
        public string FirstNameCol { get; set; }
        public string LastNameCol { get; set; }
        public string IdCol { get; set; }
        public string EmailCol { get; set; }
    }
}
