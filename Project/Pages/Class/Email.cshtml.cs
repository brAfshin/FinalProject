using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Web;
using ProjectManagement.Application.Contracts.Class;
using ProjectManagement.Application.Contracts.Email;

namespace Project.Pages.Class
{
    [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
    public class EmailModel : PageModel
    {
        private readonly CookieOptions _cookieOptions;
        private readonly IEmailApplication _emailApplication;

        public List<ContactViewModel> Contacts { get; set; }
        public string SelectedId { get; set; }

        public EmailModel( CookieOptions cookieOptions, IEmailApplication emailApplication)
        {
            _cookieOptions = cookieOptions;
            _emailApplication = emailApplication;
        }

        public async Task OnGet(string id)
        {
            var path = _cookieOptions.GetCookies("path");
            var module = _cookieOptions.GetCookies("module");
            Contacts = await _emailApplication.GetContacts(path, module);
            SelectedId = "No Contact";
            if (!string.IsNullOrEmpty(id))
            {
                var sentContact = Contacts.FirstOrDefault(x => x.Id.Contains(id));
                if (sentContact != null)
                    SelectedId = sentContact.Id;
                
            }
            
        }

        public IActionResult OnPostSendMail(List<string> emails,string subject, string body)
        {
            var sender = _cookieOptions.GetCookies("email");
            var email = new NewEmail()
            {
                Body = body,
                Recipients = emails,
                Sender = sender,
                Subject = subject

            };
            try
            {
                var result = _emailApplication.SendEmail(email);
                if (result.IsSucceeded)
                    TempData["ErrorMessage"] = "<div class=\"alert alert-success\" role=\"alert\">Email sent.</div>";
                else
                    TempData["ErrorMessage"] = $"<div class=\"alert alert-danger\" role=\"alert\">{result.Message}</div>";
                
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] =  "<div class=\"alert alert-danger\" role=\"alert\">An error has been occurred.</div>";
            }


            return RedirectToPage("./Email");

        }
    }
}

