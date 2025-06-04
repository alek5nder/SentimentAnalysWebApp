using Microsoft.AspNetCore.HttpOverrides;
using WebAppAI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ProjektTI.Models;
using Microsoft.AspNetCore.Authentication.Cookies;


var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear(); // Akceptuj wszystkie IP
    options.KnownProxies.Clear();
});

// nowe
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(30); // Cookie lifetime
    });

builder.Services.AddAuthorization();

// nowe


builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");


builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

builder.Services.AddHttpClient();

builder.Services.AddDbContext<SentimentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SentimentDb")));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseForwardedHeaders();

app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("pl")
    .AddSupportedCultures("pl", "en")
    .AddSupportedUICultures("pl", "en"));

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseStatusCodePages(async context => {
    var response = context.HttpContext.Response;
    if (response.StatusCode == 405)
    {
        response.Redirect("/Error/LanguageChangeNotAllowed");
    }
    if (response.StatusCode == 404 || response.StatusCode == 403 || response.StatusCode == 500)
    {
        response.Redirect($"/Error/{response.StatusCode}");
    }
});

app.UseRouting();
app.UseAuthorization();
app.UseAuthentication();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Sentiment}/{action=Index}/{id?}");

app.Run();
