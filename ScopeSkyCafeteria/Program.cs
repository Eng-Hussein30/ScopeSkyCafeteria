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
using Minio;
using ScopeSkyCafeteria.Services.Implementations;
using ScopeSkyCafeteria.Services.Interfaces;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// Database
// ==========================================

string dbConnection =
    Environment.GetEnvironmentVariable("DB_CONNECTION")
    ?? throw new Exception("DB_CONNECTION is missing.");

builder.Services.AddDbContext<SSCafeteriaDbContext>(options =>
    options.UseNpgsql(
        dbConnection,
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()
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

builder.Services.AddAutoMapper(
    cfg => { },
    typeof(AutoMapperProfiles));

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
// MinIO
// ==========================================

string minioEndpoint =
    Environment.GetEnvironmentVariable("MINIO_ENDPOINT")
    ?? throw new Exception("MINIO_ENDPOINT is missing.");

string minioAccessKey =
    Environment.GetEnvironmentVariable("MINIO_ACCESS_KEY")
    ?? throw new Exception("MINIO_ACCESS_KEY is missing.");

string minioSecretKey =
    Environment.GetEnvironmentVariable("MINIO_SECRET_KEY")
    ?? throw new Exception("MINIO_SECRET_KEY is missing.");

bool minioUseSsl =
    bool.TryParse(
        Environment.GetEnvironmentVariable("MINIO_USE_SSL"),
        out var useSsl)
        && useSsl;

builder.Services.AddSingleton<IMinioClient>(_ =>
    new MinioClient()
        .WithEndpoint(minioEndpoint)
        .WithCredentials(minioAccessKey, minioSecretKey)
        .WithSSL(minioUseSsl)
        .Build());

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
builder.Services.AddScoped<IFileStorageService, MinioFileStorageService>();
builder.Services.AddHttpClient<ITelegramNotificationService, TelegramNotificationService>();

// ==========================================
// OpenAPI
// ==========================================

builder.Services.AddOpenApi();

var app = builder.Build();

// ==========================================
// OpenAPI + Scalar
// ==========================================

app.MapOpenApi();
app.MapScalarApiReference();

// ==========================================
// Middleware
// ==========================================

app.UseHttpsRedirection();

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
