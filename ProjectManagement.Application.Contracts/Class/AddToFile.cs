namespace ProjectManagement.Application.Contracts.Class
{
    public class AddToFile
    {
        public List<string> Columns { get; set; }
        public int StartRow { get; set; }
        public string IdCol { get; set; }
        public string Module { get; set; }
        public string SourcePath { get; set; }
        public string FilePath { get; set; }
        public string Type { get; set; }
        public bool Replace { get; set; }
    }
}
