using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Web;
using ProjectManagement.Application.Contracts.Class;

namespace Project.Pages.Class
{
    [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
    public class TemplateModel : PageModel
    {
        private readonly IClassApplication _classApplication;
        private readonly CookieOptions _cookieOptions;
        public TemplateModel(IClassApplication classApplication, CookieOptions cookieOptions)
        {
            _classApplication = classApplication;
            _cookieOptions = cookieOptions;
        }

        public List<ReportItem> Headers { get; set; }
        public List<ReportItem> Mark { get; set; }
        public List<ReportItem> Attendance { get; set; }
        public List<ReportItem> Checkpoint { get; set; }
        public async Task OnGet()
        {
            var path = _cookieOptions.GetCookies("path");
            var module = _cookieOptions.GetCookies("module");
            var result = await _classApplication.GetReportItem(path, module);

            Headers = result.Where(x => x.table.Contains("info") && x.title != "Id").ToList();
            Attendance = result.Where(x => x.table.Contains("attendance") && x.title != "Id").ToList();
            Mark = result.Where(x => x.table.Contains("mark") && x.title != "Id").ToList();
            Checkpoint = result.Where(x => x.table.Contains("checkpoint") && x.title != "Id").ToList();
        }

        public async Task<IActionResult> OnPostDownload(string path,string table,int irow,string icol, string gcol)
        {
            var source = table.Split("---");
            var template = new CopyMark()
            {
                DbPath = _cookieOptions.GetCookies("path"),
                Module = _cookieOptions.GetCookies("module"),
                Table = source[0],
                ColId = icol,
                ColMark = gcol,
                ReportPath = path,
                StartRowId = irow,
                Select = source[1]

        };
            var result = await _classApplication.UpdateTemplate(template);
            result.Position = 0;
            return File(result, "application/octet-stream", "template.xlsx");

        }
    }
}
