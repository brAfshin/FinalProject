using System.Data;

namespace ProjectManagement.Application.Contracts.Class
{
    public class ConfirmationModel
    {
        public List<string> NewHeaders { get; set; }
        public DataTable OldData { get; set; }
        public DataTable NewData { get; set; }
    }
}
