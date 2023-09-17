using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Web;
using ProjectManagement.Application.Contracts.Class;

namespace Project.Pages.Class
{
    [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]

    public class Add : PageModel
    {
        private readonly IClassApplication _classApplication;
        private readonly CookieOptions _cookieOptions;
        public Add(IClassApplication classApplication, CookieOptions cookieOptions)
        {
            _classApplication = classApplication;
            _cookieOptions = cookieOptions;
        }

        public List<ReportItem> Headers { get; set; }
        public async Task OnGet()
        {
            var path = _cookieOptions.GetCookies("path");
            var module = _cookieOptions.GetCookies("module");
            Headers = await _classApplication.GetReportItem(path, module);
        }

        public async Task<IActionResult> OnPostAddToDb(string filePath, int firstRow, string idCol, string cols, string replace, string type)
        {


            try
            {

                bool isReplace = !(replace == "on");
                var path = _cookieOptions.GetCookies("path");
                var module = _cookieOptions.GetCookies("module");
                var command = new AddToFile()
                {
                    Columns = cols.Split(',').ToList(),
                    IdCol = idCol,
                    SourcePath = path,
                    FilePath = filePath,
                    Module = module,
                    StartRow = firstRow,
                    Replace = isReplace,
                    Type = type
                };
                await _classApplication.AddToDd(command);

                TempData["ErrorMessage"] = "<div class=\"alert alert-success\" role=\"alert\">Data added.</div>";


            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = $"<div class=\"alert alert-danger\" role=\"alert\">{e.Message}</div>";
            }
            return RedirectToAction("");



        }
    }
}
