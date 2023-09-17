namespace ProjectManagement.Domain
{
    public class Group
    {
        public Group(string name, string year, string path)
        {
            Name = name;
            Year = year;
            Path = path;
        }

        public string Name { get; private set; }
        public string Year { get; private set; }

        public string Path { get; private set; }
 
    }
}
