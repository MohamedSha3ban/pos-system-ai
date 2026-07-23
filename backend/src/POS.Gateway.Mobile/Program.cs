using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using POS.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Same composition root as the other two gateways -- see POS.Gateway.Admin/Program.cs.
// This project exposes only what the Flutter staff app actually calls: login, product/
// category CRUD, checkout, reorder suggestions, Stripe create-intent. No Users/Roles/
// Tenants/Inventory-list -- those live only in the Admin gateway.
builder.Services.AddInfrastructure(builder.Configuration);

// No CORS policy needed -- native mobile apps aren't subject to browser CORS. If you add
// a mobile *web* build (Flutter web) later, add a CORS policy here the same way the other
// two gateways do.

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
    // No Database.Migrate() here -- POS.Gateway.Admin is the designated migrations owner.
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
