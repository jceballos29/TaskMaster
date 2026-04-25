using Microsoft.AspNetCore.Mvc.ApplicationModels;
using TaskMaster.Api.Configuration;
using TaskMaster.Api.Middleware;
using TaskMaster.Application;
using TaskMaster.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var mediatRLicense = builder.Configuration["MediatR:LicenseKey"];

var config = builder.Configuration;

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowNextJsClient",
        policy =>
        {
            var clientUrl = config["ClientUrl"] ?? "http://localhost:3000";

            policy.WithOrigins(clientUrl).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
    );
});

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

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowNextJsClient");

app.MapControllers();

app.MapHealthChecks("/api/health");
app.MapFallbackToFile("/index.html");

app.Run();
