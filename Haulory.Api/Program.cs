using Haulory.Application.Features.Drivers;
using Haulory.Application.Features.Vehicles.CreateVehicleSet;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Application.Services;
using Haulory.Infrastructure.Persistence;
using Haulory.Infrastructure.Persistence.Repositories;
using Haulory.Infrastructure.Persistence.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Haulory.Api",
        Version = "v1"
    });

    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

builder.Services.AddDbContextFactory<HauloryDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services.AddScoped<IUserAccountRepository, UserAccountRepository>();
builder.Services.AddScoped<IDriverRepository, DriverRepository>();
builder.Services.AddScoped<IDriverInductionRepository, DriverInductionRepository>();
builder.Services.AddScoped<IWorkSiteRepository, WorkSiteRepository>();
builder.Services.AddScoped<IInductionRequirementRepository, InductionRequirementRepository>();
builder.Services.AddScoped<CreateVehicleHandler>();
builder.Services.AddScoped<IVehicleAssetRepository, VehicleAssetRepository>();
builder.Services.AddScoped<IComplianceEnsurer, ComplianceEnsurer>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IDeliveryReceiptRepository, DeliveryReceiptRepository>();
builder.Services.AddScoped<CreateDriverHandler>();
builder.Services.AddScoped<CreateDriverFromUserHandler>();

var jwtKey = builder.Configuration["Jwt:Key"]
             ?? throw new InvalidOperationException("JWT key is missing.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("JWT issuer is missing.");
var jwtAudience = builder.Configuration["Jwt:Audience"]
                  ?? throw new InvalidOperationException("JWT audience is missing.");

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();