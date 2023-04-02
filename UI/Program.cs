using Microsoft.AspNetCore.Authentication.Cookies;
using Polly;
using Polly.Contrib.WaitAndRetry;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UI.Infrastructure.API;

var builder = WebApplication.CreateBuilder(args);

string uriApi = builder.Configuration["UriApi"];
string uriApiBot = builder.Configuration["UriApiBot"];

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();

builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<IAuthenticationClient, ApiAuthenticationClient>();
builder.Services.AddSingleton<IAdminClient, ApiAdminClient>();
builder.Services.AddSingleton<IAdminAgencyClient, ApiAdminAgencyClient>();
builder.Services.AddSingleton<IOperatorClient, ApiOperatorClient>();
builder.Services.AddSingleton<IUserClient, ApiUserClient>();
builder.Services.AddSingleton<ICabinetClient, ApiCabinetClient>();
builder.Services.AddSingleton<IChatClient, ApiChatClient>();
builder.Services.AddSingleton<ISiteClient, ApiSiteClient>();
builder.Services.AddSingleton<ICommentClient, ApiCommentClient>();
builder.Services.AddSingleton<IGroupClient, ApiGroupClient>();

builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri(uriApi);
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = ValidateServerCetification;
    }
    return handler;
}).AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 1)));

builder.Services.AddHttpClient("apiBot", client =>
{
    client.BaseAddress = new Uri(uriApiBot);
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    var hadler = new HttpClientHandler();
    if (builder.Environment.IsDevelopment())
    {
        hadler.ServerCertificateCustomValidationCallback = ValidateServerCetification;
    }
    return hadler;
}).AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 1)));

bool ValidateServerCetification(HttpRequestMessage arg1, X509Certificate2 arg2, X509Chain arg3, SslPolicyErrors arg4)
{
    return true;
}

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

app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    if (response.StatusCode == (int)HttpStatusCode.Unauthorized)
    {
        response.Redirect("/Account/LogOut");
    }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
