using Microsoft.EntityFrameworkCore;
using Sample_DTR_API.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Connection string to the database
builder.Services.AddDbContext<SampleDtrDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("ConnString")));

//Sets up CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
                          policy =>
                          {
                              policy.WithOrigins("192.168.131.93:44341")
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                          });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//To allow using the CORS Policy
app.UseCors("AllowLocalhost");

app.UseAuthorization();

app.MapControllers();

app.Run();
