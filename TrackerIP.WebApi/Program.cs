using Microsoft.EntityFrameworkCore;
using TrackerIP.Intergrations;
using TrackerIP.WebApi;
using TrackerIP.WebApi.DatabaseModels;
using TrackerIP.WebApi.Repositories;
using TrackerIP.WebApi.Services;
using TrackerIP.WebApi.Validation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TrackerIpdbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHostedService<UpdateIPsService>();

builder.Services.AddScoped<ITrackerIPService, TrackerIPService>();
builder.Services.AddScoped<IDataBaseRepository, DatabaseRepository>();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddSingleton<IIP2CService, IP2CService>();
builder.Services.AddHttpClient<IIP2CService, IP2CService>();

builder.Services.AddSingleton<IValidationFactory, ValidationFactory>();
builder.Services.AddSingleton<IValidator<string>, IPv4AdressValidator>();
builder.Services.AddSingleton<IValidator<string[]>, CountryCodeValidator>();

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
