using System.Data;
using System.Diagnostics;
using ProjectManagement.Application.Contracts.Class;
using ProjectManagement.Application.Contracts.Email;

namespace ProjectManagement.Domain
{
    public interface IProjectInfrastructure
    {
        Task<string> CreateFolder(string name);
        Task Delete(string path);
        Task<string> GetItemId(string path);
        Task<List<Group>> GetGroups(string path);
        Task SendEmail(Email email);
        Task<Stream> UpdateTemplate(CopyMark command);
        Task<Stream> CreateReport(CreateReport command);
        Task CreateFile(string path, string name);
        Task AddModule(AddModule command);
        Task<List<string>> GetModules(string path);
        Task<List<Student>> GetStudents(string path, string module);
        Task<List<ReportItem>> GetReportItem(string path, string module);
        Task AddToDb(AddToFile command);
        Task<List<StudentProgressInfo>> GetStudentProgress(string path);
        Task<List<UpdateTable>> CheckColumns(AddToFile command);
    }
}
