using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Web;
using ProjectManagement.Application.Contracts.Class;

namespace Project.Pages.Class
{
    [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
    public class IndexModel : PageModel
    {
        private readonly IClassApplication _classApplication;
        private readonly CookieOptions _cookieOptions;

        public IndexModel(IClassApplication classApplication, CookieOptions cookieOptions)
        {
            _classApplication = classApplication;
            _cookieOptions = cookieOptions;
        }

        public string Year { get; set; }
        public string ClassName { get; set; }
        public string Path { get; set; }

        public List<string> Modules { get; set; }
        public async Task OnGet()
        {
            Year = _cookieOptions.GetCookies("year");
            ClassName = _cookieOptions.GetCookies("class");
            Path = _cookieOptions.GetCookies("path");
            Modules = await _classApplication.GetModules(Path);
        }
    }
}
