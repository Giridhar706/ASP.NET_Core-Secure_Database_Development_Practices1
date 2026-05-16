using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SecureWebApp.Data;
using SecureWebApp.Security;
using SecureWebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Anti-Forgery Token for CSRF Protection
// We store the token in a cookie
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    // For local dev without HTTPS you might set options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest,
    // but in production, enforce Always
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always; 
});

// Configure ASP.NET Core Data Protection
builder.Services.AddDataProtection();

// Setup Interceptor
builder.Services.AddSingleton<AuditInterceptor>();

// Configure DbContext with SQL Server and strict encryption settings
// Connection string enforces Encrypt=True;TrustServerCertificate=False;
var connectionString = builder.Configuration.GetConnectionString("SecureDbConnection");
builder.Services.AddDbContext<SecureAppDbContext>((sp, options) =>
{
    var interceptor = sp.GetRequiredService<AuditInterceptor>();
    options.UseSqlServer(connectionString)
           .AddInterceptors(interceptor);
});

// Configure Authentication (Cookies)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always; // Prevent MITM
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict; // Prevent CSRF
        
        // Return 401/403 for API instead of redirecting to login page
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401;
            return System.Threading.Tasks.Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = 403;
            return System.Threading.Tasks.Task.CompletedTask;
        };
    });

// Register custom services
builder.Services.AddScoped<ICryptoService, CryptoService>();
builder.Services.AddScoped(typeof(ISecureLogger<>), typeof(SecureLogger<>));
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFinancialService, FinancialService>();

// Add Swagger/OpenAPI for testing
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
