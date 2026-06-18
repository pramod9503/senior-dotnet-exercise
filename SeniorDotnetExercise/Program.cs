#pragma warning disable ASP0011
using SeniorDotnetExercise.Models;
using Microsoft.EntityFrameworkCore;
using SeniorDotnetExercise.Services;
using SeniorDotnetExercise.Abstracts;

var builder = WebApplication.CreateBuilder(args);

//Setup logging.
builder.Host.ConfigureLogging((context, logging) =>
{
    logging.ClearProviders();
    logging.AddConsole();

    if (context.HostingEnvironment.IsDevelopment()) 
    {
        logging.AddDebug();
    }
});

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Setup database context and dependency injection for the invoice service.
builder.Services.AddDbContext<ExerciseDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IInvoiceService, InvoiceService>();

//For production, restrict origins with .WithOrigins("https://yourclient.com")
//instead of .AllowAnyOrigin().
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

//Seed the database with initial data if it is empty.
SeedInvoices.EnsurePopulated(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

//This Program class is for integration testing purpose.
public partial class Program { } 
