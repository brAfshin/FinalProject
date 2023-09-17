namespace ProjectManagement.Domain
{
    public class Email
    {
        public Email(List<string> recipients, string subject, string body, string sender)
        {
            Recipients = recipients;
            Subject = subject;
            Body = body;
            Sender = sender;
        }

        public List<string> Recipients { get;private set; }
        public string Subject { get; private set; }
        public string Body { get; private set; }
        public string Sender { get; private set; }


    }
}
