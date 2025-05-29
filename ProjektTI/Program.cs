using WebAppAI.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

var supportedCultures = new[] { "pl", "en" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("pl")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);



builder.Services.AddHttpClient();
builder.Services.AddDbContext<SentimentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SentimentDb")));


var app = builder.Build();
app.UseRequestLocalization(localizationOptions);


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Sentiment}/{action=Index}/{id?}");

app.Run();
