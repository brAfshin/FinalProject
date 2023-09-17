namespace ProjectManagement.Application.Contracts.Class
{
    public class CopyMark
    {
        public string ReportPath { get; set; }
        public string DbPath { get; set; }
        public string Module{ get; set; }
        public string Table{ get; set; }
        public string Select { get; set; }
        public string ColId { get; set; }
        public int StartRowId { get; set; }
        public string ColMark { get; set; }


    }
}
