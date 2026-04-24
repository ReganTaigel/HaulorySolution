using HaulitCore.Api.Controllers;
using HaulitCore.Api.Jobs;
using HaulitCore.Api.Services;
using HaulitCore.Application.Features.Drivers;
using HaulitCore.Application.Features.Jobs;
using HaulitCore.Application.Features.Reports;
using HaulitCore.Application.Features.Vehicles;
using HaulitCore.Application.Features.Vehicles.CreateVehicleSet;
using HaulitCore.Application.Features.Vehicles.UpdateVehicleSet;
using HaulitCore.Application.Interfaces.Repositories;
using HaulitCore.Application.Interfaces.Services;
using HaulitCore.Infrastructure.Persistence;
using HaulitCore.Infrastructure.Persistence.Repositories;
using HaulitCore.Infrastructure.Persistence.Services;
using HaulitCore.Infrastructure.Services;
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
        Title = "HaulitCore.Api",
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

    builder.Services.AddDbContext<HaulitCoreDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

    builder.Services.AddScoped<IUserAccountRepository, UserAccountRepository>();
    builder.Services.AddScoped<IDriverRepository, DriverRepository>();
    builder.Services.AddScoped<IDriverInductionRepository, DriverInductionRepository>();
    builder.Services.AddScoped<IWorkSiteRepository, WorkSiteRepository>();
    builder.Services.AddScoped<IInductionRequirementRepository, InductionRequirementRepository>();
    builder.Services.AddScoped<IVehicleAssetRepository, VehicleAssetRepository>();
    builder.Services.AddScoped<IComplianceEnsurer, ComplianceEnsurer>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IJobRepository, JobRepository>();
    builder.Services.AddScoped<IDeliveryReceiptRepository, DeliveryReceiptRepository>();
    builder.Services.AddScoped<IDocumentSettingsRepository, DocumentSettingsRepository>();
    builder.Services.AddScoped<IVehicleDayRunRepository, VehicleDayRunRepository>();
    builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
    builder.Services.AddScoped<IInvoiceCalculationService, InvoiceCalculationService>();

    builder.Services.AddScoped<DeleteVehicle>();
    builder.Services.AddScoped<CreateVehicleHandler>();
    builder.Services.AddScoped<CreateJobHandler>();
    builder.Services.AddScoped<CreateDriverHandler>();
    builder.Services.AddScoped<CreateDriverFromUserHandler>();
    builder.Services.AddScoped<UpdateVehicleSetHandler>();

    builder.Services.AddScoped<JobWorkflowService>();
    builder.Services.AddScoped<JobRequestValidator>();
    builder.Services.AddScoped<JobResponseFactory>();
    builder.Services.AddScoped<IInductionEvidenceFileStorage, InductionEvidenceFileStorage>();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

    builder.Services.AddTransient<InvoiceReportHandler>();
    builder.Services.AddTransient<PodReportHandler>();
    builder.Services.AddTransient<IPdfInvoiceGenerator, PdfInvoiceGenerator>();
    builder.Services.AddTransient<IPdfPodGenerator, PdfPodGenerator>();

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();

// Enable Swagger in both Development and Production for now
app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// Simple root endpoint
app.MapGet("/", () => Results.Ok("HaulitCore API is running"));

// Simple health endpoint
app.MapGet("/health", () => Results.Ok("Healthy"));

app.MapControllers();

app.Run();