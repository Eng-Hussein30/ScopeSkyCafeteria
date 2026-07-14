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

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// Database
// ==========================================

builder.Services.AddDbContext<SSCafeteriaDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ScopeSkyCafeteriaConnectionStrings"),
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
    builder.Configuration["Jwt:Key"]
    ?? "YourFallbackDefaultSuperLongSecretKey123!";


var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));

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

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

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
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
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

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// ==========================================
// Database Migration + Seeding
// ==========================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var db = services.GetRequiredService<SSCafeteriaDbContext>();

    await db.Database.MigrateAsync();

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var userManager = services.GetRequiredService<UserManager<User>>();

    await RoleSeeding.SeedRolesAsync(roleManager);
    await UserSeeding.SeedUsersAsync(userManager);
}

// ==========================================
// Run
// ==========================================

app.Run();