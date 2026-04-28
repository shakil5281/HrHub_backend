using ERPBackend.Core.Constants;
using ERPBackend.Core.Interfaces;
using ERPBackend.Core.Models;
using ERPBackend.Core.Entities;
using ERPBackend.Infrastructure.Data;
using ERPBackend.Services.Services;
using ERPBackend.Services.Interfaces;
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
#pragma warning disable CA1416
builder.Services.AddScoped<IZkTecoService, ZkTecoService>();
#pragma warning restore CA1416
builder.Services.AddScoped<IOrderSheetService, OrderSheetService>();
builder.Services.AddScoped<IMerchandisingMasterService, MerchandisingMasterService>();
builder.Services.AddScoped<ICostingService, CostingService>();
builder.Services.AddScoped<INightBillService, NightBillService>();
builder.Services.AddScoped<IAccessoryMatrixService, AccessoryMatrixService>();

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
                    .AllowCredentials()
                    .WithExposedHeaders("Content-Disposition", "Content-Length", "X-Content-Type-Options");
            }
            else
            {
                policyBuilder.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithExposedHeaders("Content-Disposition", "Content-Length", "X-Content-Type-Options");
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
        var seededCompany = await context.Companies.FirstOrDefaultAsync();
        if (seededCompany == null)
        {
            seededCompany = new Company
            {
                CompanyNameEn = "Ekushe Fashions Ltd",
                CompanyNameBn = "একুশে ফ্যাশনস লিমিটেড",
                AddressEn = "Masterbari, Gazipur, Bangladesh",
                AddressBn = "মাস্টারবাড়ী, গাজীপুর, বাংলাদেশ",
                PhoneNumber = "01711-353535",
                RegistrationNo = "REG-1001",
                Email = "ekusheit@mridhagroup-bd.com",
                Industry = "Garments",
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };
            context.Companies.Add(seededCompany);
            await context.SaveChangesAsync();
        }
        var companyId = seededCompany.Id;

        // Seed Leave Types
        if (!context.LeaveTypes.Any())
        {
            var leaveTypes = new List<LeaveType>
            {
                new LeaveType { Name = "Casual Leave", Code = "CL", YearlyLimit = 10, IsCarryForward = false, Description = "Standard annual casual leave", IsActive = true },
                new LeaveType { Name = "Sick Leave", Code = "SL", YearlyLimit = 14, IsCarryForward = false, Description = "Medical/Sick leave with certificate", IsActive = true },
                new LeaveType { Name = "Earned Leave", Code = "EL", YearlyLimit = 18, IsCarryForward = true, Description = "Annual earned leave", IsActive = true },
                new LeaveType { Name = "Maternity Leave", Code = "ML", YearlyLimit = 112, IsCarryForward = false, Description = "Maternity leave for female employees", IsActive = true }
            };
            context.LeaveTypes.AddRange(leaveTypes);
            await context.SaveChangesAsync();
        }

        // Seed Default Colors for Merchandising
        var merchContext = scope.ServiceProvider.GetRequiredService<MerchandisingDbContext>();
        if (!merchContext.FabricColorPantones.Any())
        {
            var colors = new List<FabricColorPantone>
            {
                new FabricColorPantone { ColorName = "DTM", PantoneCode = "DTM", CompanyId = companyId, BranchId = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
                new FabricColorPantone { ColorName = "BLACK", PantoneCode = "19-4008 TCX", CompanyId = companyId, BranchId = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
                new FabricColorPantone { ColorName = "WHITE", PantoneCode = "11-0601 TCX", CompanyId = companyId, BranchId = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
                new FabricColorPantone { ColorName = "NAVY", PantoneCode = "19-4023 TCX", CompanyId = companyId, BranchId = 1, IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            merchContext.FabricColorPantones.AddRange(colors);
            await merchContext.SaveChangesAsync();
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
            
            // Assign company to user
            if (seededCompany != null)
            {
                user.AssignedCompanies.Add(seededCompany);
            }

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

            // Ensure company is assigned if missing
            var userWithCompanies = await userManager.Users
                .Include(u => u.AssignedCompanies)
                .FirstOrDefaultAsync(u => u.Id == adminUser.Id);

            if (userWithCompanies != null && seededCompany != null && !userWithCompanies.AssignedCompanies.Any(c => c.Id == seededCompany.Id))
            {
                userWithCompanies.AssignedCompanies.Add(seededCompany);
                await userManager.UpdateAsync(userWithCompanies);
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
