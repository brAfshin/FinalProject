using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using ProjectManagement.Application.Contracts.Class;
using System.IO;

namespace Project.Pages.Dashboard
{
    [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
    public class IndexModel : PageModel
    {

        private readonly CookieOptions _cookieOptions;
        private readonly GraphProfileClient _graphProfileClient;
        private readonly IClassApplication _classApplication;

        public IndexModel(GraphProfileClient graphProfileClient, CookieOptions cookieOptions, IClassApplication classApplication)
        {
            _graphProfileClient = graphProfileClient;
            _cookieOptions = cookieOptions;
            _classApplication = classApplication;
        }

        public List<GroupViewModel> classes { get; set; }
        public async Task OnGet()
        {

            var user = await _graphProfileClient.GetUserProfile();
            _cookieOptions.SetCookies("name",user.DisplayName);
            _cookieOptions.SetCookies("email",user.Mail);
            classes = await _classApplication.GetClasses();


        }

        public IActionResult OnGetDelete(string path)
        {
            
            try
            {
                var result = _classApplication.DeleteClass(path);
                if (result.IsSucceeded)
                    TempData["ErrorMessage"] = "<div class=\"alert alert-success\" role=\"alert\">Group deleted.</div>";
                else
                    TempData["ErrorMessage"] = $"<div class=\"alert alert-danger\" role=\"alert\">{result.Message}</div>";

            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = "<div class=\"alert alert-danger\" role=\"alert\">An error has been occurred.</div>";
            }
            return RedirectToAction("");
        }

        public IActionResult OnGetCreate()
        {
            return Partial("./Create");
            
        }


        public async Task<IActionResult> OnGetLogout()
        {

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect(
               "https://login.microsoftonline.com/common/oauth2/v2.0/logout?post_logout_redirect_uri=https://localhost:7116/");

        }
        public async Task<IActionResult> OnGetView(string path, string year, string name)
        {


            _cookieOptions.SetCookies("path", path);
            _cookieOptions.SetCookies("year", year);
            _cookieOptions.SetCookies("class", name);
            return Redirect("/Class/Index");

        }

        public IActionResult OnGetCreateGroup()
        {
            return Partial("./CreateGroup");

        }

        public async Task<IActionResult> OnPostCreateGroup(CreateGroup data)
        {


            try
            {
                var result = await _classApplication.CreateNewGroup(data);
                if (result.IsSucceeded)
                    TempData["ErrorMessage"] = "<div class=\"alert alert-success\" role=\"alert\">Class created.</div>";
                else
                    TempData["ErrorMessage"] = $"<div class=\"alert alert-danger\" role=\"alert\">{result.Message}</div>";

            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = "<div class=\"alert alert-danger\" role=\"alert\">An error has been occurred.</div>";
            }
            return RedirectToAction("");
        }

        public IActionResult OnGetCreateModule(string path)
        {
            var command = new AddModule()
            {
                dbPath = path
            };
            return Partial("./CreateModule",command);

        }

        public async Task<IActionResult> OnGetShowModule(string path)
        {
            var command = await _classApplication.GetModules(path);
            return Partial("./ShowModule", command);

        }


        public async Task<IActionResult> OnPostCreateModule(AddModule data)
        {
            var result = await _classApplication.AddModule(data);

            try
            {
                
                if (result.IsSucceeded)
                    TempData["ErrorMessage"] = "<div class=\"alert alert-success\" role=\"alert\">Module created.</div>";
                else
                    TempData["ErrorMessage"] = $"<div class=\"alert alert-danger\" role=\"alert\">{result.Message}</div>";

            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = "<div class=\"alert alert-danger\" role=\"alert\">An error has been occurred.</div>";
            }
            return RedirectToAction("");
        }
    }
}
