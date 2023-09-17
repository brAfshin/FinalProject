using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Web;
using ProjectManagement.Application.Contracts.Class;

namespace Project.Pages.Class
{
    [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
    public class StudentProgressModel : PageModel
    {
        private readonly IClassApplication _classApplication;
        private readonly CookieOptions _cookieOptions;



        public StudentProgressModel(IClassApplication classApplication, CookieOptions cookieOptions)
        {
            _classApplication = classApplication;
            _cookieOptions = cookieOptions;

        }

        public List<StudentProgressInfo> Info { get; set; }
        public async Task OnGet()
        {
            var path = _cookieOptions.GetCookies("path");
            var result = await _classApplication.GetStudentProgress(path);
            Info = result;
        }
    }
}
