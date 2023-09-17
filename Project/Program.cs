using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Project;
using ProjectManagement.Application;
using ProjectManagement.Application.Contracts.Class;
using ProjectManagement.Domain;
using ProjectManagement.Infrastructure;
using CookieOptions = Project.CookieOptions;
using WebApplication = Microsoft.AspNetCore.Builder.WebApplication;
var builder = WebApplication.CreateBuilder(args);

string[] initialScopes =
    builder.Configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');



builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration)
    .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
    .AddMicrosoftGraph(builder.Configuration.GetSection("DownstreamApi"))
    .AddInMemoryTokenCaches();


builder.Services
    .AddRazorPages()
    .AddMicrosoftIdentityUI();

builder.Services.AddScoped<GraphProfileClient>();

builder.Services.AddScoped<CookieOptions>();

builder.Services.AddScoped<IProjectInfrastructure, ProjectInfrastructure>();
builder.Services.AddScoped<IClassApplication,ClassApplication>();
builder.Services.AddScoped<IEmailApplication,EmailApplication>();





var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
