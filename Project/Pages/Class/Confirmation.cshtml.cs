using System.Data;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Graph;
using ProjectManagement.Application.Contracts.Class;
using System.Drawing;
using Microsoft.Identity.Web;

namespace Project.Pages.Class
{
    [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
    public class ConfirmationModel : PageModel
    {
        private readonly IClassApplication _classApplication;
        private readonly CookieOptions _cookieOptions;

        public ConfirmationModel(IClassApplication classApplication, CookieOptions cookieOptions)
        {
            _classApplication = classApplication;
            _cookieOptions = cookieOptions;
        }
        public List<string> SimilarHeaders { get; set; }
        public List<UpdateTable> Result { get; set; }
        public AddToFile Command { get; set; }
        public string ColList { get; set; }
        public async Task OnGet(string filePath, int firstRow, string idCol, string cols, string replace, string type)
        {
            try
            {
                bool isReplace = !(replace == "on");
                var path = _cookieOptions.GetCookies("path");
                var module = _cookieOptions.GetCookies("module");
                ColList = cols;
                Command = new AddToFile()
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
                Result = await _classApplication.CheckColumns(Command);

            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = $"<div class=\"alert alert-danger\" role=\"alert\">{e.Message}</div>";

            }



        }

        public async Task<IActionResult> OnPostAddToDb(string filePath, int firstRow, string idCol, string cols, string replace, string type)
        {


            try
            {


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
                    Replace = true,
                    Type = type
                };
                await _classApplication.AddToDd(command);


                TempData["ErrorMessage"] = "<div class=\"alert alert-success\" role=\"alert\">Data added.</div>";


            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = $"<div class=\"alert alert-danger\" role=\"alert\">{e.Message}</div>";
            }
            return RedirectToPage("./Add");



        }
    }
}
