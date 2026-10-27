using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using PrivacyGuard.Data;
using PrivacyGuard.Interfaces;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Repositories;
using PrivacyGuard.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
// 1. Register Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Yeh line infinite loop ko ignore kar degi
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Yahan apna token daalein. Example: 'eyJhbGciOiJIUz...'"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 2. Configure JWT Authentication
var jwtKey = builder.Configuration["JwtSettings:SecurityKey"]
    ?? throw new InvalidOperationException(
        "JWT SecurityKey is missing from configuration.");

var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]
    ?? throw new InvalidOperationException(
        "JWT Issuer is missing from configuration.");

var jwtAudience = builder.Configuration["JwtSettings:Audience"]
    ?? throw new InvalidOperationException(
        "JWT Audience is missing from configuration.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// 3. Configure SQL Server Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// 4. Register Repository and Service Dependencies
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<ITenantService, TenantService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IAuditService, AuditService>();

builder.Services.AddScoped<IBreachRepository, BreachRepository>();
builder.Services.AddScoped<IBreachService, BreachService>();

builder.Services.AddScoped<ICampaignRepository, CampaignRepository>();
builder.Services.AddScoped<ICampaignService, CampaignService>();

builder.Services.AddScoped<IConsentRepository, ConsentRepository>();
builder.Services.AddScoped<IConsentService, ConsentService>();

builder.Services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();

builder.Services.AddScoped<IPrivacyRequestRepository, PrivacyRequestRepository>();
builder.Services.AddScoped<IPrivacyRequestService, PrivacyRequestService>();

// Data Inventory dependencies.
// Add these if DataInventoryController and DataInventoryService
// are still part of your project.
builder.Services.AddScoped<IDataInventoryRepository, DataInventoryRepository>();
builder.Services.AddScoped<IDataInventoryService, DataInventoryService>();

builder.Services.AddScoped<IUserDashboardRepository, UserDashboardRepository>();
builder.Services.AddScoped<IUserDashboardService, UserDashboardService>();

builder.Services.AddScoped<IOrgUnitRepository, OrgUnitRepository>();
builder.Services.AddScoped<IOrgUnitService, OrgUnitService>();

builder.Services.AddScoped<IDpiaRepository, DpiaRepository>();
builder.Services.AddScoped<IDpiaService, DpiaService>();

builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();

builder.Services.AddScoped<IGrievanceRepository, GrievanceRepository>();
builder.Services.AddScoped<IGrievanceService, GrievanceService>();

builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IStaffService, StaffService>();
// 5. Configure OpenAPI
//builder.Services.AddOpenApi();

var app = builder.Build();

// 6. Configure HTTP Request Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    //app.MapOpenApi();
}

app.UseHttpsRedirection();

// Authentication must run before Authorization.
app.UseAuthentication();
app.UseAuthorization();

// 7. Map API Controllers
app.MapControllers();


// Seed Dummy Data for Demo
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<PrivacyGuard.Data.ApplicationDbContext>();
        PrivacyGuard.Data.DataSeeder.Initialize(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine("An error occurred while seeding the database: " + ex.Message);
    }
}

app.Run();
app.Run();
