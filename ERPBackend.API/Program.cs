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

builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IStoreService, StoreService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICuttingService, CuttingService>();

if (OperatingSystem.IsWindows())


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

builder.Services.AddControllers();
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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}

// app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllers();

// Seed Roles
try
{
    using (var scope = app.Services.CreateScope())
    {
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
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Seed Company
            Company? company = await dbContext.Companies.FirstOrDefaultAsync();
            if (company == null)
            {
                company = new Company
                {
                    CompanyNameEn = "HrHub Demo Corp",
                    CompanyNameBn = "এইচআরহাব ডেমো কর্প",
                    AddressEn = "Dhaka, Bangladesh",
                    AddressBn = "ঢাকা, বাংলাদেশ",
                    PhoneNumber = "0123456789",
                    RegistrationNo = "REG-001",
                    Industry = "Software",
                    Email = "info@hrhub.com",
                    Status = "Active",
                    Founded = 2024,
                    CreatedAt = DateTime.UtcNow
                };
                dbContext.Companies.Add(company);
                await dbContext.SaveChangesAsync();
            }

            // Seed Departments
            if (!await dbContext.Departments.AnyAsync())
            {
                dbContext.Departments.AddRange(
                    new Department { NameEn = "IT", NameBn = "আইটি", CompanyId = company.Id },
                    new Department { NameEn = "HR", NameBn = "এইচআর", CompanyId = company.Id },
                    new Department { NameEn = "Finance", NameBn = "ফিন্যান্স", CompanyId = company.Id }
                );
                await dbContext.SaveChangesAsync();
            }

            var itDept = await dbContext.Departments.FirstAsync(d => d.NameEn == "IT");
            var hrDept = await dbContext.Departments.FirstAsync(d => d.NameEn == "HR");

            // Seed Sections
            if (!await dbContext.Sections.AnyAsync())
            {
                dbContext.Sections.AddRange(
                    new Section { NameEn = "Development", NameBn = "ডেভেলপমেন্ট", CompanyId = company.Id, DepartmentId = itDept.Id },
                    new Section { NameEn = "Recruitment", NameBn = "রিক্রুটমেন্ট", CompanyId = company.Id, DepartmentId = hrDept.Id }
                );
                await dbContext.SaveChangesAsync();
            }

            var devSection = await dbContext.Sections.FirstAsync(s => s.NameEn == "Development");
            var hrSection = await dbContext.Sections.FirstAsync(s => s.NameEn == "Recruitment");

            // Seed Designations
            if (!await dbContext.Designations.AnyAsync())
            {
                dbContext.Designations.AddRange(
                    new Designation { NameEn = "Software Engineer", NameBn = "সফটওয়্যার ইঞ্জিনিয়ার", CompanyId = company.Id, DepartmentId = itDept.Id, SectionId = devSection.Id },
                    new Designation { NameEn = "HR Manager", NameBn = "এইচআর ম্যানেজার", CompanyId = company.Id, DepartmentId = hrDept.Id, SectionId = hrSection.Id }
                );
                await dbContext.SaveChangesAsync();
            }

            var devDesig = await dbContext.Designations.FirstAsync(d => d.NameEn == "Software Engineer");

            // Seed Groups
            if (!await dbContext.Groups.AnyAsync())
            {
                dbContext.Groups.AddRange(
                    new Group { NameEn = "Worker", NameBn = "শ্রমিক", CompanyId = company.Id },
                    new Group { NameEn = "Staff", NameBn = "স্টাফ", CompanyId = company.Id }
                );
            }

            // Seed Floors
            if (!await dbContext.Floors.AnyAsync())
            {
                dbContext.Floors.AddRange(
                    new Floor { NameEn = "Ground", NameBn = "নিচ তলা", CompanyId = company.Id },
                    new Floor { NameEn = "1st", NameBn = "১ম তলা", CompanyId = company.Id }
                );
            }

            // Seed Demo Employees for Dashboard Logic
            if (!await dbContext.Employees.AnyAsync())
            {
                var today = DateTime.UtcNow;
                dbContext.Employees.AddRange(
                    new Employee
                    {
                        EmployeeId = "EMP001",
                        FullNameEn = "John Doe",
                        FullNameBn = "জন ডো",
                        DepartmentId = itDept.Id,
                        DesignationId = devDesig.Id,
                        JoinDate = today.AddYears(-2).AddDays(10), // Anniversary in 10 days
                        DateOfBirth = today.AddYears(-30).AddDays(5), // Birthday in 5 days
                        Status = "Active",
                        IsActive = true,
                        CompanyId = company.Id,
                        Gender = "Male"
                    },
                    new Employee
                    {
                        EmployeeId = "EMP002",
                        FullNameEn = "Jane Smith",
                        FullNameBn = "জেন স্মিথ",
                        DepartmentId = hrDept.Id,
                        DesignationId = (await dbContext.Designations.FirstAsync(d => d.NameEn == "HR Manager")).Id,
                        JoinDate = today.AddYears(-1).AddDays(2), // Anniversary in 2 days
                        DateOfBirth = today.AddYears(-25).AddDays(20), // Birthday in 20 days
                        Status = "Active",
                        IsActive = true,
                        CompanyId = company.Id,
                        Gender = "Female"
                    }
                );
            }

            await dbContext.SaveChangesAsync();
        }
    }
catch (Exception ex)
{
    // Log exception or ignore if database doesn't exist yet
    Console.WriteLine("Seeding failed (likely due to missing DB): " + ex.Message);
}

app.Run();
