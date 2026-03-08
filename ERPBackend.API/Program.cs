using ERPBackend.Core.Constants;
using ERPBackend.Core.Interfaces;
using ERPBackend.Core.Models;
using ERPBackend.Core.Entities;
using ERPBackend.Infrastructure.Data;
using ERPBackend.Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; // Re-add this
using System.Text;

// Configure EPPlus and QuestPDF licenses
// Licenses are set in the controllers or via configuration
// OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
// QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);
OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// Add services to the container

// 1. EF Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions => { sqlOptions.EnableRetryOnFailure(); }));

builder.Services.AddDbContext<CashbookDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CashbookConnection"),
        sqlServerOptionsAction: sqlOptions => { sqlOptions.EnableRetryOnFailure(); }));

builder.Services.AddDbContext<ProductionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductionConnection"),
        sqlServerOptionsAction: sqlOptions => { sqlOptions.EnableRetryOnFailure(); }));

builder.Services.AddDbContext<StoreDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StoreConnection"),
        sqlServerOptionsAction: sqlOptions => { sqlOptions.EnableRetryOnFailure(); }));

builder.Services.AddDbContext<MerchandisingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MerchandisingConnection"),
        sqlServerOptionsAction: sqlOptions => { sqlOptions.EnableRetryOnFailure(); }));

builder.Services.AddDbContext<CuttingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CuttingConnection"),
        sqlServerOptionsAction: sqlOptions => { sqlOptions.EnableRetryOnFailure(); }));

// 2. Identity
// 2. Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 3;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 3. Custom Services
// 4. Custom Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDatabaseService, DatabaseService>();
// Register Services
builder.Services.AddScoped<IStoreService, StoreService>();
builder.Services.AddScoped<IMerchandisingService, MerchandisingService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICuttingService, CuttingService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IZkTecoService, ZkTecoService>();
builder.Services.AddScoped<IOrderSheetService, OrderSheetService>();
builder.Services.AddScoped<IMerchandisingMasterService, MerchandisingMasterService>();
builder.Services.AddScoped<ICostingService, CostingService>();

// 4. Authentication
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JWT:ValidAudience"],
            ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
            IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"] ?? ""))
        };
    });

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddEndpointsApiExplorer();

// CORS configuration
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" };
    options.AddPolicy("AllowAll",
        policyBuilder =>
        {
            if (builder.Environment.IsDevelopment())
            {
                policyBuilder.SetIsOriginAllowed(origin => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }
            else
            {
                policyBuilder.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithExposedHeaders("Content-Disposition");
            }
        });
});

// 5. Swagger with Auth
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ERP Backend API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] { }
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
// Enable Swagger in all environments (app runs as Production from local publish)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

// app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Seed Data
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Ensure at least one company exists
        if (!context.Companies.Any())
        {
            context.Companies.Add(new Company
            {
                CompanyNameEn = "Default Company",
                CompanyNameBn = "ডিফল্ট কোম্পানি",
                AddressEn = "Default Address",
                AddressBn = "ডিফল্ট ঠিকানা",
                PhoneNumber = "0123456789",
                RegistrationNo = "REG-1001",
                Email = "info@hrhub.com",
                Industry = "Textile",
                Status = "Active"
            });
            await context.SaveChangesAsync();
        }

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var roles = new[]
        {
            UserRoles.SuperAdmin, UserRoles.Admin, UserRoles.ItOfficer, UserRoles.Accounts,
            UserRoles.HrManager, UserRoles.HrOfficer,
            UserRoles.AccountGm, UserRoles.AccountOfficer,
            UserRoles.StoreGm, UserRoles.StoreManager, UserRoles.StoreOfficer, UserRoles.StoreAdmin,
            UserRoles.Production, UserRoles.ProductionManager, UserRoles.Merchandiser, UserRoles.Cutting
        };

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed SuperAdmin User
        var adminUser = await userManager.FindByNameAsync("superadmin");
        if (adminUser == null)
        {
            var user = new ApplicationUser
            {
                UserName = "superadmin",
                Email = "admin@hrhub.com",
                FullName = "Super Admin",
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await userManager.CreateAsync(user, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "SuperAdmin");
            }
        }
        else
        {
            if (!await userManager.IsInRoleAsync(adminUser, "SuperAdmin"))
            {
                await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
            }
        }
    }
}
catch (Exception ex)
{
    // Log exception or ignore if database doesn't exist yet
    Console.WriteLine("Seeding failed (likely due to missing DB): " + ex.Message);
}

app.Run();
