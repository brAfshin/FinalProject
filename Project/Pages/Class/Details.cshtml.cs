using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Web;
using ProjectManagement.Application.Contracts.Class;

namespace Project.Pages.Class
{
    [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
    public class DetailsModel : PageModel
    {


        private readonly IClassApplication _classApplication;
        private readonly CookieOptions _cookieOptions;


        public DetailsModel(IClassApplication classApplication, CookieOptions cookieOptions)
        {
            _classApplication = classApplication;
            _cookieOptions = cookieOptions;
        }
        public StudentProgressInfo Info { get; set; }
        public async Task OnGet(string id)
        {
            var path = _cookieOptions.GetCookies("path");
            var result = await _classApplication.GetStudentProgress(path);
            Info = result.FirstOrDefault(x=>x.Id == id);
        }
    }
}
