using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using YildizCanAdmin.Web.Components;
using YildizCanAdmin.Web.Components.Account;
using YildizCanAdmin.Web.Data;
using YildizCanAdmin.Web.Services;

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// İç panel: gerçek e-posta gönderici yok (no-op), doğrulama sahte olurdu — kapalı.
// Register'dan sonra doğrudan giriş. (Açık kayıt kısıtı ayrı bir iş: bkz. README.)
builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// YıldızCan Node admin API istemcisi. BaseAddress '/' ile bitmeli; ADMIN_KEY
// gizli, appsettings'e değil user-secrets/ortam değişkenine konur ve yalnızca
// sunucuda kalır (tarayıcıya inmez).
builder.Services.AddHttpClient<YildizCanApiClient>((sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = cfg["YildizCan:ApiBaseUrl"]
        ?? throw new InvalidOperationException("YildizCan:ApiBaseUrl tanımlı değil");
    client.BaseAddress = new Uri(baseUrl);
    var adminKey = cfg["YildizCan:AdminKey"];
    if (!string.IsNullOrEmpty(adminKey))
        client.DefaultRequestHeaders.Add("x-admin-key", adminKey);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.MapGet("/reports/student/{user}/{id}", async (string user, string id, YildizCanApiClient api) =>
{
    var detail = await api.GetStudentAsync(user, id);
    if (detail is null) return Results.NotFound();
    var sessions = await api.GetSessionsAsync(user, id)
        ?? new YildizCanAdmin.Shared.SessionsResponse([], []);
    var pdf = ParentReportPdf.Generate(detail, sessions);
    return Results.File(pdf, "application/pdf", $"veli-raporu-{id}.pdf");
}).RequireAuthorization();

app.Run();
