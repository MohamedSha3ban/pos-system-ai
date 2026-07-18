using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using POS.Infrastructure;
using POS.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Composition root: wires up the Identity, Catalog, Orders, Payments, and Insights
// modules (see POS.Infrastructure/DependencyInjection.cs).
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    // Admin portal (web-admin), storefront (web-storefront), + Flutter web/mobile clients.
    options.AddPolicy("Clients", policy =>
        policy.WithOrigins("http://localhost:4200", "http://localhost:4201")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // Auto-migrate in dev for convenience; use proper migration deploys in prod.
    // Only WriteDbContext ever runs migrations -- see WriteDbContext's doc comment.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseCors("Clients");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
