namespace ProjectManagement.Domain
{
    public class Student
    {
        public Student(string id, string email, string forename, string surname)
        {
            Id = id;
            Email = email;
            Forename = forename;
            Surname = surname;
        }

        public string Id { get; private set; }
        public string Email { get; private set; }
        public string Forename { get; private set; }
        public string Surname { get; private set; }





    }
}
