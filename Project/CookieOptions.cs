namespace Project
{
    public  class CookieOptions : HttpContextAccessor
    {
        // Set a cookie
        public  void SetCookies(string id, string content)
        {
            Microsoft.AspNetCore.Http.CookieOptions options = new Microsoft.AspNetCore.Http.CookieOptions
            {
                // Set expiry time
                Expires = DateTime.Now.AddMonths(1),
                IsEssential = true,
                //The location of the cookie
                Path = "/"
            };

            HttpContext.Response.Cookies.Append(id, content, options);
        }

        // Retrieve data from cookie
        public string GetCookies(string id)
        {
            var value = HttpContext.Request.Cookies[id];
            return value ?? string.Empty;
        }
    }
}

