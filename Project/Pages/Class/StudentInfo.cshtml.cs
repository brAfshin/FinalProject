using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Web;
using ProjectManagement.Application;
using ProjectManagement.Application.Contracts.Class;

namespace Project.Pages.Class
{
    [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
    public class StudentInfoModel : PageModel
    {

        private readonly IClassApplication _classApplication;
        private readonly CookieOptions _cookieOptions;


        public StudentInfoModel(IClassApplication classApplication, CookieOptions cookieOptions)
        {
            _classApplication = classApplication;
            _cookieOptions = cookieOptions;
        }
        public List<StudentViewModel> Students { get; set; }

        public async Task OnGet()
        {
            var path = _cookieOptions.GetCookies("path");
            var module = _cookieOptions.GetCookies("module");
            Students = await _classApplication.GetStudents(path,module);
        }
    }
}
