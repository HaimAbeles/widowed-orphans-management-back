using Microsoft.EntityFrameworkCore;
using Serilog;
using WidowedOrphansManagement.Data.Contexts;
using WidowedOrphansManagement.Services.ExcelService;

var builder = WebApplication.CreateBuilder(args);

// ����� Serilog ���� appsettings.json
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact",
        policy =>
        {
            policy.WithOrigins(builder.Configuration["CorsOrigin"])
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// ����� �������� �� EF
builder.Services.AddDbContext<WidowedOrphansContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ����� ������ �-ExcelService
builder.Services.AddScoped<IExcelService, ExcelService>();
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<WidowedOrphansContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

var app = builder.Build();

app.UseCors("AllowReact");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseSerilogRequestLogging(); // ��� ������� ��� ������

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
