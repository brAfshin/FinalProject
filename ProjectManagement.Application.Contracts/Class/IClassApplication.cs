using System.Data;

namespace ProjectManagement.Application.Contracts.Class
{
    public interface IClassApplication
    {

        Task<List<GroupViewModel>> GetClasses();

        OperationResult DeleteClass(string id);

        Task<Stream> UpdateTemplate(CopyMark copyMark);

        Task<OperationResult> CreateNewGroup(CreateGroup command);
        Task<OperationResult> AddModule(AddModule command);

        Task<Stream> CreateReport(CreateReport command);

        Task<List<string>> GetModules(string path);

        Task<List<StudentViewModel>> GetStudents(string path, string module);

        Task<List<ReportItem>> GetReportItem(string path, string module);

        Task AddToDd(AddToFile command);

        Task<List<StudentProgressInfo>> GetStudentProgress(string path);

        Task<List<UpdateTable>> CheckColumns(AddToFile command);
    }
}
