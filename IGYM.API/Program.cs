using IGYM.Data;
using IGYM.Interface;
using IGYM.Interface.Data;
using IGYM.Interface.GymModule;
using IGYM.Interface.UserModule;
using IGYM.Model;
using IGYM.Service;
using IGYM.Service.GymModule;
using IGYM.Service.UserModule;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration
Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Information()
	.WriteTo.Console()
	.WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
	.CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailNotification, EmailNotification>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<TrackLogin>();
builder.Services.AddTransient<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IMfaService, MfaService>();
builder.Services.AddScoped<ILoginData, LoginData>();
builder.Services.AddScoped<IUserHistoryService, UserHistoryService>();
builder.Services.AddScoped<IGymSheduleService, GymScheduleService>();
builder.Services.AddScoped<IGymNutritionService, GymNutritionService>();
/*builder.Configuration
	.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
	.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
	.AddEnvironmentVariables();*/

builder.Services.AddDbContext<IGYMDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
		b => b.MigrationsAssembly("IGYM.Model")));

builder.Services.AddRateLimiter(options =>
{
	options.AddPolicy("LoginLimit", context =>
		RateLimitPartition.GetFixedWindowLimiter(
			context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
			_ => new FixedWindowRateLimiterOptions
			{
				PermitLimit = 5,
				Window = TimeSpan.FromMinutes(1),
				QueueLimit = 0
			}));
});
var tempProvider = builder.Services.BuildServiceProvider();
var encryptionService = tempProvider.GetRequiredService<IEncryptionService>();
string encryptedJwtKey = builder.Configuration["Jwt:Key"]!;
string decryptedJwtKey = encryptionService.DecryptData(encryptedJwtKey);

builder.Services.AddAuthentication(options =>
{
	options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidIssuer = builder.Configuration["Jwt:Issuer"],
		ValidateAudience = true,
		ValidAudience = builder.Configuration["Jwt:Audience"],
		ValidateLifetime = true,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(decryptedJwtKey)),
		NameClaimType = JwtRegisteredClaimNames.Name
	};

	// JWT validation event
	options.Events = new JwtBearerEvents
	{
		OnTokenValidated = context =>
		{
			var token = context.SecurityToken as JwtSecurityToken;
			if (token != null)
			{
				var claimsIdentity = context.Principal.Identity as ClaimsIdentity;

				// Validate required claims (email, name, and phone number)
				var emailClaim = claimsIdentity?.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
				var nameClaim = claimsIdentity?.FindFirst(JwtRegisteredClaimNames.Name)?.Value;
				var phoneNumberClaim = claimsIdentity?.FindFirst(ClaimTypes.MobilePhone)?.Value;
				var userIdClaim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				var roleClaim = claimsIdentity?.FindFirst(ClaimTypes.Role)?.Value;

				if (string.IsNullOrEmpty(emailClaim) || string.IsNullOrEmpty(nameClaim) || string.IsNullOrEmpty(phoneNumberClaim))
				{
					context.Fail("Unauthorized: Missing required claims.");
					return Task.CompletedTask;
				}
			}
			return Task.CompletedTask;
		}
	};
});
/*
builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
	options.AddPolicy("SalesPersonOnly", policy => policy.RequireRole("SalesPerson"));
	options.AddPolicy("SalesCoordinatorOnly", policy => policy.RequireRole("SalesCoordinator"));
});*/
builder.Services.AddCors(o =>
{
	o.AddPolicy("AllowSpecificOrigin", build =>
	{
		build.WithOrigins("http://207.180.217.101:3000/", "http://localhost:3000")
			 .AllowAnyMethod()
			 .AllowAnyHeader()
			 .AllowCredentials();
	});
});


builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();



app.UseHttpsRedirection();

app.UseRateLimiter(); // Enable rate limiting middleware

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();