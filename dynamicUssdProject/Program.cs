using dynamicUssdProject.REPO;
using dynamicUssdProject.Data; // Make sure to include this for MongoDbContext
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver; // Ensure you have this for IMongoClient
using System;
using dynamicUssdProject.Models;

// Create a builder for the web application.
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Configure SQL Server database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register MongoDB settings
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

// Register MongoDB client
builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
    new MongoClient(builder.Configuration["MongoDbSettings:ConnectionString"]));
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull; // Optionally ignore null values
    });

// Register MongoDbContext
builder.Services.AddScoped<MongoDbContext>();

// Register your repositories
builder.Services.AddScoped<UssdMenuService>();
builder.Services.AddScoped<UserPinRepository>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<UserRepository>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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
app.UseAuthorization();
app.MapControllers();
app.Run();