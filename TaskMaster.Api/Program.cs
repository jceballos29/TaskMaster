using Microsoft.AspNetCore.Mvc.ApplicationModels;
using TaskMaster.Api.Configuration;
using TaskMaster.Application;
using TaskMaster.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var mediatRLicense = builder.Configuration["MediatR:LicenseKey"];

var config = builder.Configuration;

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddControllers(options =>
{
    options.Conventions.Add(
        new RouteTokenTransformerConvention(new KebabCaseParameterTransformer())
    );
});
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddApplication(mediatRLicense);
builder.Services.AddInfrastructure(config);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapControllers();

app.MapHealthChecks("/api/health");
app.MapFallbackToFile("/index.html");

app.Run();
