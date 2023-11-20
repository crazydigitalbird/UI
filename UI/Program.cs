using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UI.Infrastructure;
using UI.Infrastructure.API;
using UI.Infrastructure.Filters;
using UI.Infrastructure.Hubs;
using UI.Infrastructure.Repository;
using UI.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

string uriApi = builder.Configuration["UriApi"];
string uriApiBot = builder.Configuration["UriApiBot"];

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
});

builder.Services.AddSingleton<IDictionaryRepository<SheetDialogKey, NewMessage>, DictionaryChatRepository>();

builder.Services.AddSingleton<ChatServices>();
builder.Services.AddHostedService<BackgroundServiceStarter<ChatServices>>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();

builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<IAuthenticationClient, ApiAuthenticationClient>();
builder.Services.AddSingleton<IAdminClient, ApiAdminClient>();
builder.Services.AddSingleton<IAdminAgencyClient, ApiAdminAgencyClient>();
builder.Services.AddSingleton<IOperatorClient, ApiOperatorClient>();
builder.Services.AddSingleton<ISheetClient, ApiSheetClient>();
builder.Services.AddSingleton<ICabinetClient, ApiCabinetClient>();
builder.Services.AddSingleton<IChatClient, ApiChatClient>();
builder.Services.AddSingleton<ISiteClient, ApiSiteClient>();
builder.Services.AddSingleton<ICommentClient, ApiCommentClient>();
builder.Services.AddSingleton<IGroupClient, ApiGroupClient>();
builder.Services.AddSingleton<IBalanceClient, ApiBalanceClient>();
builder.Services.AddSingleton<IUserClient, ApiUserClient>();
builder.Services.AddSingleton<IMailClient, ApiMailClient>();
builder.Services.AddSingleton<IIcebreakersClient, ApiIcebreakersClient>();
builder.Services.AddSingleton<IMediaClient, ApiMediaClient>();
builder.Services.AddSingleton<IAutorespondersClient, ApiAutorespondersClient>();
builder.Services.AddSingleton<IWorkingShiftClient, ApiWorkingShiftClient>();
builder.Services.AddSingleton<IStatisticClient, ApiStatisticClient>();
builder.Services.AddScoped<GetAgencyIdFilter>();
builder.Services.AddScoped<UpdateSessionAttribute>();

builder.Services.AddTransient<IRazorPartialToStringRenderer, RazorPartialToStringRenderer>();
builder.Services.AddScoped<IChatHub, CallingSideChatHub>();

builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri(uriApi);
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = ValidateServerCetification
    };
    if (builder.Environment.IsDevelopment())
    {
    }
    return handler;
})/*.AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 1)))*/;

builder.Services.AddHttpClient("apiBot", client =>
{
    client.BaseAddress = new Uri(uriApiBot);
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    var hadler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = ValidateServerCetification
    };
    if (builder.Environment.IsDevelopment())
    {
    }
    return hadler;
})/*.AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 1)))*/;

builder.Services.AddSignalR(conf =>
{
    conf.MaximumReceiveMessageSize = null;
    conf.EnableDetailedErrors = true;
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

static bool ValidateServerCetification(HttpRequestMessage arg1, X509Certificate2 arg2, X509Chain arg3, SslPolicyErrors arg4)
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

app.MapHub<ChatHub>("/chatHub");

//app.MapGet("/.well-known/acme-challenge/2DXkfA__W_ZbIjB5fe1v4MwJ0UfVuf3_BweBYlm9aVg", () => "2DXkfA__W_ZbIjB5fe1v4MwJ0UfVuf3_BweBYlm9aVg.yUlg7Rq3NLu8bSYDhPnsGB_VTOkopaT3M-5pmjgEb7o");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseSession();

app.Run();
