using FluentValidation;
using Serilog;
using ZZLD_Form.API.Middleware;
using ZZLD_Form.Core.Models;
using ZZLD_Form.Core.Services;
using ZZLD_Form.Core.Validators;
using ZZLD_Form.Infrastructure.Configuration;
using PdfImpl = ZZLD_Form.Infrastructure.Pdf;
using StorageImpl = ZZLD_Form.Infrastructure.Storage;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/zzld-form-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "ZZLD Form Generator API",
            Version = "v1",
            Description = "API for generating ZZLD (Personal Data Protection) forms for Bulgarian citizens"
        });

        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    // Configure options
    builder.Services.Configure<AzureStorageOptions>(
        builder.Configuration.GetSection(AzureStorageOptions.SectionName));

    // Register validators
    builder.Services.AddScoped<IValidator<PersonalData>, PersonalDataValidator>();

    // Register services
    builder.Services.AddScoped<IFormService, FormService>();
    builder.Services.AddScoped<ITemplateService, TemplateService>();
    builder.Services.AddScoped<IPdfProcessor, PdfImpl.PdfProcessor>();
    builder.Services.AddScoped<IBlobStorageService, StorageImpl.BlobStorageService>();

    // Add CORS
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline
    app.UseErrorHandling();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseCors();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("Starting ZZLD Form Generator API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class public for integration tests
public partial class Program { }
