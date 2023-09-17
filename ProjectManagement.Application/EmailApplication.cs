using ProjectManagement.Application.Contracts;
using ProjectManagement.Application.Contracts.Class;
using ProjectManagement.Application.Contracts.Email;
using ProjectManagement.Domain;

namespace ProjectManagement.Application
{
    public class EmailApplication : IEmailApplication
    {
        private readonly IProjectInfrastructure _projectInfrastructure;

        public EmailApplication(IProjectInfrastructure projectInfrastructure)
        {
            _projectInfrastructure = projectInfrastructure;
        }

        public async Task<List<ContactViewModel>> GetContacts(string path, string module)
        {
            var contacts = await _projectInfrastructure.GetStudents(path,module);
            return contacts.Select(x => new ContactViewModel()
            {
                Id = x.Id,
                Email = x.Email,
                Name = $"{x.Surname} {x.Forename}"
            }).ToList();

        }

        public OperationResult SendEmail(NewEmail email)
        {
            var newMail = new Email(email.Recipients, email.Subject, email.Body, email.Sender);
            OperationResult result = new OperationResult();
            try
            {
                _projectInfrastructure.SendEmail(newMail);
                return result.Succeeded();
            }
            catch (Exception e)
            {
                 return result.Failed(e.Message);
            }
                

        }
    }
}
