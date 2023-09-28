using API.ErrorHandling;
using DataAccess;
using DataAccess.Implement;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Service;
using Service.Implement;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<PlatformAntiquesHandicraftsContext>(options => options.UseSqlServer("name=ConnectionStrings:dev"));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<IUserDAO, UserDAO>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryDAO, CategoryDAO>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IMaterialDAO, MaterialDAO>();
builder.Services.AddScoped<IMaterialService, MaterialService>();
var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.ConfigureExceptionHandler(logger);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
