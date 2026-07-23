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

// Composition root shared by all three gateways: wires up every module (Identity,
// Catalog, Orders, Payments, Insights, Storefront) and MediatR. Only the controllers in
// each gateway project differ -- this project exposes the full staff/back-office surface.
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Clients", policy =>
        policy.WithOrigins("http://localhost:4200") // web-admin only
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
    // Only WriteDbContext ever runs migrations -- see WriteDbContext's doc comment. This
    // gateway is designated the "migrations owner": Ecommerce and Mobile don't call
    // Migrate() at all, assuming the schema is already current by the time they start.
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
