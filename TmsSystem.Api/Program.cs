using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using TmsSystem.Api.Auth;
using TmsSystem.Api.Responses;
using TmsSystem.Application;
using TmsSystem.Infrastructure;
using TmsSystem.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values
                .SelectMany(entry => entry.Errors)
                .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage) ? "Invalid request." : error.ErrorMessage)
                .ToArray();

            return new BadRequestObjectResult(ApiResponse<object>.FailureResponse(
                StatusCodes.Status400BadRequest,
                "Validation failed.",
                errors));
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "TMS System API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = Array.Empty<string>()
    });
});
builder.Services.AddAuthentication("TmsJwt")
    .AddScheme<AuthenticationSchemeOptions, TmsJwtAuthenticationHandler>("TmsJwt", _ => { });
builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("TmsClient", policy => policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(_ => true));
});

var app = builder.Build();

if (builder.Configuration.GetValue("Database:InitializeOnStartup", true))
{
    using var scope = app.Services.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<TmsDbInitializer>();
    await initializer.InitializeAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("TmsClient");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
