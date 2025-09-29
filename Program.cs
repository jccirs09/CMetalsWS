using CMetalsWS.Components;
using CMetalsWS.Components.Account;
using CMetalsWS.Data;
using CMetalsWS.Data.Chat;
using CMetalsWS.Security;
using CMetalsWS.Services;
using CMetalsWS.Services.SignalR;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MudBlazor.Services;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using CMetalsWS.Hubs;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient("OpenAI", c =>
{
    c.BaseAddress = new Uri("https://api.openai.com/");
    c.Timeout = TimeSpan.FromMinutes(2);
});

// MudBlazor
builder.Services.AddMudServices();

// Razor Components (Blazor Server)
builder.Services.AddRazorComponents()
.AddInteractiveServerComponents(options => { options.DetailedErrors = true; });

// Identity + AuthN plumbing for Razor Components
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

// EF Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sql =>
    {
        sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null);
        sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    });

    if (builder.Environment.IsDevelopment())
    {
        options.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
    }
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Storage", LogLevel.Information);

// IdentityCore with Roles
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddRoles<ApplicationRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

// Claims principal factory to load role permission claims into the user
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AppClaimsPrincipalFactory>();

builder.Services.Configure<IdentityOptions>(o =>
{
    o.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;
    o.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
    o.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
});

// Refresh claims quickly after role/permission changes
builder.Services.Configure<SecurityStampValidatorOptions>(o =>
{
    o.ValidationInterval = TimeSpan.FromMinutes(1);
});

// Policy-based authorization for fine-grained permissions
builder.Services.AddAuthorization(options =>
{
    foreach (var perm in Permissions.All())
    {
        options.AddPolicy(perm, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(Permissions.ClaimType, perm);
        });
    }
    options.AddPolicy("Manager", policy => policy.RequireRole("Manager"));
});

// App services
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
builder.Services.AddScoped<BranchService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ShiftService>();
builder.Services.AddScoped<MachineService>();
builder.Services.AddScoped<TruckService>();
builder.Services.AddScoped<PickingListService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<ItemRelationshipService>();
builder.Services.AddTransient<IdentityDataSeeder>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<LoadService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<ITaskAuditEventService, TaskAuditEventService>();
builder.Services.AddScoped<DestinationGroupService>();
builder.Services.AddScoped<DestinationRegionService>();
builder.Services.AddScoped<WorkOrderService>();
builder.Services.AddSingleton<RegionAssignmentService>();

// Picking List PDF Parser
builder.Services.AddScoped<IPdfParsingService, PdfParsingService>();
builder.Services.AddScoped<IPickingListImportService, PickingListImportService>();
builder.Services.AddScoped<IParsingStateService, ParsingStateService>();


//builder.Services.AddScoped<ChatStateService>();
//builder.Services.A`dScoped<ChatHubClient>();
//builder.Services.AddScoped<NotificationService>();
//builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});


var app = builder.Build();

// Seed roles, permission claims, and admin user
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IdentityDataSeeder>();
    await seeder.SeedAsync();
}

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Identity /Account endpoints for Razor Components
app.MapAdditionalIdentityEndpoints();

app.MapHub<ScheduleHub>("/hubs/schedule");
//app.MapHub<ChatHub>("/chathub");



app.Run();