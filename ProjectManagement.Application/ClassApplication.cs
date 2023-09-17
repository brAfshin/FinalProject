using System.Data;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using ProjectManagement.Application.Contracts;
using ProjectManagement.Application.Contracts.Class;
using ProjectManagement.Domain;

namespace ProjectManagement.Application
{
    public class ClassApplication : IClassApplication
    {
        private readonly IProjectInfrastructure _projectInfrastructure;


        public ClassApplication(IProjectInfrastructure projectInfrastructure)
        {
            _projectInfrastructure = projectInfrastructure;
        }


        public async Task<List<GroupViewModel>> GetClasses()
        {
            // Find Id of db folder.
             var dbId = await _projectInfrastructure.GetItemId("db");

             // If db not found return an empty list
            if (string.IsNullOrEmpty(dbId))
                return new List<GroupViewModel>();

            // Get Groups
            var query =await _projectInfrastructure.GetGroups(dbId);
            return query.Select(c => new GroupViewModel
            {

                Name = c.Name,
                Path = c.Path,
                Year = c.Year

            }).ToList();
        }

        public OperationResult DeleteClass(string id)
        {
            var result = new OperationResult();
            try
            {
                _projectInfrastructure.Delete(id);
                return result.Succeeded();
            }
            catch (Exception e)
            {
                return result.Failed(e.Message);
            }
            
        }
        
        public async Task<Stream> UpdateTemplate(CopyMark copyMark)
        {


            var result = await _projectInfrastructure.UpdateTemplate(copyMark);

            
            return result;
        }

        public async Task<OperationResult> CreateNewGroup(CreateGroup command)
        {
            var result = new OperationResult();
            try
            {
                // check whether db folder is already available or not if not create it
                var dbId = await _projectInfrastructure.GetItemId("db");
                if (string.IsNullOrEmpty(dbId))
                    dbId = await _projectInfrastructure.CreateFolder("db");

                // Create a file base on group name and year
                var name = $"{command.Year}-{command.Name}.xlsx";
                await _projectInfrastructure.CreateFile(dbId, name);
                return result.Succeeded();
            }
            catch (Exception e)
            {
                return result.Failed(e.Message);
            }
        }

        public async Task<OperationResult> AddModule(AddModule command)
        {
            var result = new OperationResult();
            // Add a new sheet to a group Excel file
            await _projectInfrastructure.AddModule(command);
            return result.Succeeded();
      

 
        }
        public Task<Stream> CreateReport(CreateReport command)
        {
            return _projectInfrastructure.CreateReport(command);
        }


        public async Task<List<string>> GetModules(string path)
        {
            // Get the sheet list and remove the info sheet (it is automatically create at first time)
            var result = await _projectInfrastructure.GetModules(path);
            if (result.Contains("info"))
                result.Remove("info");
            return result;


        }

        public async Task<List<StudentViewModel>> GetStudents(string path, string module)
        {
            // Get list of students and return a new view model based on it
            var result = await _projectInfrastructure.GetStudents(path, module);
            return result.Select(x => new StudentViewModel()
            {

                Email = x.Email,
                FirstName = x.Surname,
                Id = x.Id,
                LastName = x.Forename
            }).ToList();
        }

        public async Task<List<ReportItem>> GetReportItem(string path, string module)
        {
            return await _projectInfrastructure.GetReportItem( path,  module);
        }

        public async Task AddToDd(AddToFile command)
        {
            await _projectInfrastructure.AddToDb(command);
        }

        public Task<List<StudentProgressInfo>> GetStudentProgress(string path)
        {
            return _projectInfrastructure.GetStudentProgress(path);
        }

        public async Task<List<UpdateTable>> CheckColumns(AddToFile command)
        {
            return await _projectInfrastructure.CheckColumns(command);
        }
    }
}
