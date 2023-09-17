namespace ProjectManagement.Application.Contracts.Class
{
    public class StudentViewModel
    {
        public string Id { get;  set; }
        public string Email { get;  set; }
        public string FirstName { get;  set; }
        public string LastName { get;  set; }
        public List<Dictionary<string, string>> Attributes { get;  set; }
    }
}
