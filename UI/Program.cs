using Microsoft.AspNetCore.Authentication.Cookies;
using Polly;
using Polly.Contrib.WaitAndRetry;
using UI.Infrastructure.API;

var builder = WebApplication.CreateBuilder(args);

string uriApi = builder.Configuration["UriApi"];

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();

builder.Services.AddSingleton<IAuthenticationClient, ApiAuthenticationClient>();
builder.Services.AddSingleton<IAdminClient, ApiAdminClient>();
builder.Services.AddSingleton<IAdminAgencyClient, ApiAdminAgencyClient>();
builder.Services.AddSingleton<IOperatorClient, ApiOperatorClient>();
builder.Services.AddSingleton<IUserClient, ApiUserClient>();

builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri(uriApi);
}).AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5)));

var app = builder.Build();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
