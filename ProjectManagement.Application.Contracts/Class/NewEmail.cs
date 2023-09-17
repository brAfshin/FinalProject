namespace ProjectManagement.Application.Contracts.Class
{
    public class NewEmail
    {
        public List<string> Recipients { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Sender { get; set; }


    }
}
