using Repository.Interfaces;
using Repository;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

//// Add JWT configuration:
//var jwtSettings = builder.Configuration.GetSection("JwtSettings");
//var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var jwtSettings = configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "default-secret-key";
var key = Encoding.UTF8.GetBytes(secretKey);


// Add Authentication:
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Add authorization:
builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Repository for Dependency Injection (Move it before `builder.Build()`)
builder.Services.AddScoped<IAccountRepository, AccountRepository>();

builder.Services.AddScoped<IFlightRepository, FlightRepository>();

builder.Services.AddHttpClient<IGuestRepository, GuestRepository>();

builder.Services.AddScoped<IBookingRepository, BookingRepository>();

var stripeSettings = builder.Configuration.GetSection("StripeSetting");
StripeConfiguration.ApiKey = stripeSettings["SecretKey"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()   // Allows requests from any origin
            .AllowAnyMethod()   // Allows any HTTP method (GET, POST, PUT, DELETE, etc.)
            .AllowAnyHeader();  // Allows any HTTP headers
    });
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddControllersWithViews();

var handler = new HttpClientHandler { UseCookies = true };
var _httpClient = new HttpClient(handler);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowAll"); // Ensure CORS policy is defined

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "user",
    pattern: "{controller=Account}/{action=Register}",
    defaults: new { contoller = "Account"});

app.MapControllerRoute(
    name: "flights",
    pattern: "{controller=Flight}/{action=Details}/{id?}",
    defaults: new { controller = "Flight" });

app.Run();
