using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using ScopeSkyCafeteria.Data;
using ScopeSkyCafeteria.Mapping;
using ScopeSkyCafeteria.Models.Domain;
using ScopeSkyCafeteria.Repositories;
using System.Text;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// Database
// ==========================================

string dbConnection =
    Environment.GetEnvironmentVariable("DB_CONNECTION")
    ?? throw new Exception("DB_CONNECTION is missing.");

builder.Services.AddDbContext<SSCafeteriaDbContext>(options =>
    options.UseSqlServer(
        dbConnection,
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));

// ==========================================
// Identity
// ==========================================

builder.Services
    .AddIdentityCore<User>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<SSCafeteriaDbContext>()
    .AddDefaultTokenProviders();

// ==========================================
// JWT Authentication
// ==========================================

string jwtSecretKey =
    Environment.GetEnvironmentVariable("JWT_KEY")
    ?? throw new Exception("JWT_KEY is missing.");

string jwtIssuer =
    Environment.GetEnvironmentVariable("JWT_ISSUER")
    ?? throw new Exception("JWT_ISSUER is missing.");

string jwtAudience =
    Environment.GetEnvironmentVariable("JWT_AUDIENCE")
    ?? throw new Exception("JWT_AUDIENCE is missing.");

var signingKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(jwtSecretKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,

            IssuerSigningKey = signingKey
        };
    });

// ==========================================
// Controllers
// ==========================================

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// ==========================================
// AutoMapper
// ==========================================

builder.Services.AddAutoMapper(typeof(AutoMapperProfiles));

// ==========================================
// CORS
// ==========================================

string frontendUrl =
    Environment.GetEnvironmentVariable("FRONTEND_URL")
    ?? throw new Exception("FRONTEND_URL is missing.");

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(frontendUrl)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ==========================================
// Dependency Injection
// ==========================================

builder.Services.AddScoped<IProductRepository, SQLProductRepository>();
builder.Services.AddScoped<ICategoryRepository, SQLCategoryRepository>();
builder.Services.AddScoped<IOrderRepository, SQLOrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, SQLOrderItemRepository>();
builder.Services.AddScoped<IUserRepository, SQLUserRepository>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddScoped<IWalletRepository, SQLWalletRepository>();

// ==========================================
// OpenAPI
// ==========================================

builder.Services.AddOpenApi();

var app = builder.Build();

// ==========================================
// Development
// ==========================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// ==========================================
// Middleware
// ==========================================

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();

app.UseCors("FrontendPolicy");

app.UseAuthorization();

app.MapControllers();

// ==========================================
// Database Migration + Seeding
// ==========================================

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var services = scope.ServiceProvider;

    // Database Context
    var dbContext =
        services.GetRequiredService<SSCafeteriaDbContext>();

    const int maxRetry = 5;

    for (int retry = 1; retry <= maxRetry; retry++)
    {
        try
        {
            await dbContext.Database.MigrateAsync();
            break;
        }
        catch
        {
            if (retry == maxRetry)
                throw;

            Console.WriteLine(
                $"Database is not ready... Retry {retry}/{maxRetry}");

            await Task.Delay(5000);
        }
    }

    // Role Manager
    var roleManager =
        services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    // User Manager
    var userManager =
        services.GetRequiredService<UserManager<User>>();

    // Seed Roles
    await RoleSeeding.SeedRolesAsync(roleManager);

    // Seed Users + Wallets
    await UserSeeding.SeedUsersAsync(
        userManager,
        dbContext);
}

// ==========================================
// Run
// ==========================================

app.Run();