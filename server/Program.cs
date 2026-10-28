using AccountingProject.Data;
using AccountingProject.Infrastructure;
using AccountingProject.Models;
using AccountingProject.Services;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
ProductionAdminBootstrap.ApplyCommandLineSwitch(args, builder.Configuration);

HostEnvironmentConfigurationValidator.Validate(builder.Environment, builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ── Rate limiting: protect /api/auth/login from brute-force ──────────────
// In development the limit is relaxed so E2E tests (many parallel logins) are not blocked.
var isDev = builder.Environment.IsDevelopment();
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", cfg =>
    {
        cfg.PermitLimit = isDev ? 200 : 10;
        cfg.Window = isDev ? TimeSpan.FromMinutes(1) : TimeSpan.FromMinutes(5);
        cfg.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        cfg.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var jwtKeyRaw = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Configuration Jwt:Key is required.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Configuration Jwt:Issuer is required.");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Configuration Jwt:Audience is required.");
var jwtSigningKeyBytes = JwtSigningKey.GetKeyBytes(jwtKeyRaw);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Must match JwtTokenService claim "role"; default inbound mapping rewrites it so RequireRole fails.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(jwtSigningKeyBytes),
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = "role",
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.AddPolicy(AuthPolicies.AdminOnly, policy =>
        policy.RequireRole(UserRoles.Admin));
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AccountingProject API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme.",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
            },
            Array.Empty<string>()
        },
    });
});

builder.AddConfiguredCors();
builder.Services.AddDbContext<PayrollDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IEmployerService, EmployerService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IEmploymentDataService, EmploymentDataService>();
builder.Services.AddScoped<IEmploymentCalculationService, EmploymentCalculationService>();
builder.Services.AddScoped<IBulkImportService, BulkImportService>();
builder.Services.AddScoped<IComparisonReportService, ComparisonReportService>();
builder.Services.AddScoped<IReportExportService, ReportExportService>();
builder.Services.AddScoped<IAnnualComparisonSavedReportService, AnnualComparisonSavedReportService>();
builder.Services.AddScoped<IPayrollMonthlyInputService, PayrollMonthlyInputService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ── Security headers (all responses) ────────────────────────────────────
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    // Content-Security-Policy: React SPA uses inline styles/scripts; tighten further when CSP nonces are added
    headers["Content-Security-Policy"] =
        "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: blob:; font-src 'self' data:; connect-src 'self';";
    await next();
});

// ── HTTPS redirect + HSTS in production ─────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

// Serve the React production build from wwwroot (single-origin hosting under IIS/Kestrel).
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRateLimiter();
app.UseCors("ReactClient");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
// SPA fallback only for client-side routes (no /api, no /assets, and no file extension paths).
app.MapFallbackToFile(
    "{*path:regex(^(?!api(?:/|$))(?!assets(?:/|$)).*):nonfile}",
    "index.html")
    .AllowAnonymous();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<PayrollDbContext>();
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    await DevelopmentAdminUserSeeder.SeedAsync(
        services.GetRequiredService<IHostEnvironment>(),
        services.GetRequiredService<IConfiguration>(),
        db,
        loggerFactory.CreateLogger("DevelopmentAdminUserSeeder"));
    await ProductionAdminBootstrap.RunIfRequestedAsync(
        services.GetRequiredService<IConfiguration>(),
        db,
        loggerFactory.CreateLogger("ProductionAdminBootstrap"));
}

app.Run();
