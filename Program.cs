using CMetalsWS.Components;
using CMetalsWS.Components.Account;
using CMetalsWS.Data;
using CMetalsWS.Security;
using CMetalsWS.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;


var builder = WebApplication.CreateBuilder(args);

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
    options.UseSqlServer(connectionString));
builder.Services.AddScoped(p => p.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

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
builder.Services.AddScoped<MachineService>();
builder.Services.AddScoped<TruckService>();
builder.Services.AddScoped<PickingListService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<ItemRelationshipService>();
builder.Services.AddTransient<IdentityDataSeeder>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<LoadService>();
builder.Services.AddScoped<WorkOrderService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<ITaskAuditEventService, TaskAuditEventService>();

builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddSignalR();


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
app.MapHub<ChatHub>("/hubs/chat");

app.MapPost("api/pdf/parse", async (IFormFile file, IPdfParsingService pdfParser, ILogger<Program> logger) =>
{
    if (file is null || file.Length == 0)
    {
        return Results.BadRequest("No file uploaded.");
    }
    if (file.Length > 25_000_000) // 25 MB limit
    {
        return Results.BadRequest("File size exceeds 25 MB limit.");
    }

    try
    {
        await using var stream = file.OpenReadStream();
        var extractionResult = await pdfParser.ParsePdfAsync(stream);
        if (extractionResult is null)
        {
            return Results.Problem("Failed to parse the PDF.");
        }
        return Results.Ok(extractionResult);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error parsing PDF for file {FileName}", file.FileName);
        return Results.Problem($"An error occurred while processing the PDF: {ex.Message}");
    }
})
.DisableAntiforgery();

app.Run();