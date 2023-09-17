
using ProjectManagement.Application.Contracts;
using ProjectManagement.Application.Contracts.Class;
using ProjectManagement.Application.Contracts.Email;

    public interface IEmailApplication
    {
        Task<List<ContactViewModel>> GetContacts(string path, string module);

        OperationResult SendEmail(NewEmail email);
    }

