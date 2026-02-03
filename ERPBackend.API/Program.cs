using ERPBackend.Core.Constants;
using ERPBackend.Core.Interfaces;
using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using ERPBackend.Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; // Re-add this
using System.Text;

// Configure EPPlus license for non-commercial use (EPPlus 8+)
OfficeOpenXml.ExcelPackage.License.SetNonCommercialOrganization("HR Hub");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

// 1. EF Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
builder.Services.AddScoped<IAuthService, AuthService>();

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
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
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
            // Ensure existing superadmin has the SuperAdmin role
            if (!await userManager.IsInRoleAsync(adminUser, "SuperAdmin"))
            {
                await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
            }
        }

        // Seed Groups and Floors
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var groups = new[] { "Worker", "Staff" };
        foreach (var groupName in groups)
        {
            if (!await dbContext.Groups.AnyAsync(g => g.NameEn == groupName))
            {
                dbContext.Groups.Add(new Group { NameEn = groupName });
            }
        }

        var floors = new[] { "Ground Floor", "1st Floor", "2nd Floor", "3rd Floor" };
        foreach (var floorName in floors)
        {
            if (!await dbContext.Floors.AnyAsync(f => f.NameEn == floorName))
            {
                dbContext.Floors.Add(new Floor { NameEn = floorName });
            }
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
