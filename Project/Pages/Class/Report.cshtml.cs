using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Web;
using ProjectManagement.Application.Contracts.Class;

namespace Project.Pages.Class
{
    [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]

    public class ReportModel : PageModel
    {
        private readonly IClassApplication _classApplication;
        private readonly CookieOptions _cookieOptions;
        public ReportModel(IClassApplication classApplication, CookieOptions cookieOptions)
        {
            _classApplication = classApplication;
            _cookieOptions = cookieOptions;
        }

        public List<ReportItem >Headers { get; set; }
        public List<ReportItem >Mark { get; set; }
        public List<ReportItem >Attendance { get; set; }
        public List<ReportItem >Checkpoint { get; set; }
        public async Task OnGet()
        {
            var path = _cookieOptions.GetCookies("path");
            var module = _cookieOptions.GetCookies("module");
            var result =  await _classApplication.GetReportItem(path, module);
            Headers = result.Where(x => x.table.Contains("info")).ToList();
            Attendance = result.Where(x => x.table.Contains("attendance") && x.title != "Id").ToList();
            Mark = result.Where(x => x.table.Contains("mark") && x.title != "Id").ToList();
            Checkpoint = result.Where(x => x.table.Contains("checkpoint") && x.title != "Id").ToList();
        }

        public async Task<IActionResult> OnPostDownload(List<string> info, List<string> mark, List<string> attendance, List<string> checkpoint)
        {
            try
            {
                var path = _cookieOptions.GetCookies("path");
                var module = _cookieOptions.GetCookies("module");
                var command = new CreateReport()
                {
                    Path = path,
                    Mark = mark,
                    Attendance = attendance,
                    Checkpoint = checkpoint,
                    Module = module,
                    Info = info
                };
                var result = await _classApplication.CreateReport(command);
                result.Position = 0;
                return File(result, "application/octet-stream", "report.xlsx");
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = $"<div class=\"alert alert-danger\" role=\"alert\">{e.Message}</div>";

                return RedirectToAction("");
            }


        }
    }
}
