using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repository.Data;
using Repository.Interfaces;
using Repository.Implementations;
using Service.Interfaces;
using Service.Implementations;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;
using API.Authorization;



var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CONFIGURE EF CORE
// ==========================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");


var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlServer(connectionString)
    .LogTo(Console.WriteLine, LogLevel.Information)
    .EnableSensitiveDataLogging()
    .Options;


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// ==========================================
// 2. CONFIGURE DEPENDENCY INJECTION
// ==========================================
builder.Services.AddScoped(typeof(IAppointmentRepository), typeof(AppointmentRepository));
builder.Services.AddScoped(typeof(IAuditLogRepository), typeof(AuditLogRepository));
builder.Services.AddScoped(typeof(IDoctorProfileRepository), typeof(DoctorProfileRepository));
builder.Services.AddScoped(typeof(IPatientProfileRepository), typeof(PatientProfileRepository));
builder.Services.AddScoped(typeof(IMedicalRecordRepository), typeof(MedicalRecordRepository));
builder.Services.AddScoped(typeof(IRefreshTokenRepository), typeof(RefreshTokenRepository));
builder.Services.AddScoped(typeof(IUserRepository), typeof(UserRepository));

builder.Services.AddScoped(typeof(IAppointmentService), typeof(AppointmentService));
builder.Services.AddScoped(typeof(IAuditLogService), typeof(AuditLogService));
builder.Services.AddScoped(typeof(IDoctorProfileService), typeof(DoctorProfileService));
builder.Services.AddScoped(typeof(IPatientProfileService), typeof(PatientProfileService));
builder.Services.AddScoped(typeof(IMedicalRecordService), typeof(MedicalRecordService));    
builder.Services.AddScoped(typeof(IUserService), typeof(UserService));


// ==========================================
// 3. CONFIGURE JWT AUTHENTICATION
// ==========================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Secret"]!);

builder.Services.AddAuthentication(options =>
{
    // Set default authentication schemes to JWT
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
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ClockSkew = TimeSpan.Zero // Removes default 5-minute clock delay for token expiration
    };
});

// Add services to the container.

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // This tells Swagger that our API uses JWT Bearer authentication
    // through the HTTP Authorization header.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        // The name of the HTTP header where the token will be sent.
        Name = "Authorization",

        // Indicates this is an HTTP authentication scheme.
        Type = SecuritySchemeType.Http,

        // Specifies the authentication scheme name.
        // Must be exactly "Bearer" for JWT Bearer tokens.
        Scheme = "Bearer",

        // Optional metadata to describe the token format.
        BearerFormat = "JWT",

        // Specifies that the token is sent in the request header.
        In = ParameterLocation.Header,

        // Text shown in Swagger UI to guide the user.
        Description = "Enter: Bearer {your JWT token}"
    });

    // This tells Swagger that endpoints protected by [Authorize]
    // require the Bearer token defined above.
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                // Reference the previously defined "Bearer" security scheme.
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },


            // No scopes are required for JWT Bearer authentication.
            // This array is empty because JWT does not use OAuth scopes here.
            new string[] {}
        }
    });
});


builder.Services.AddSingleton<IAuthorizationHandler, OwnResourceHandler>();

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
