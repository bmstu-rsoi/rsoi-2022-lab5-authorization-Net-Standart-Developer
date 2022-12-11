using GatewayLab2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Authority = "https://dev-73urcdgst7428q7h.us.auth0.com/";
    options.Audience = "https://myrsoi5lab/";
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = ClaimTypes.NameIdentifier
    };
});

builder.Services
      .AddAuthorization(options =>
      {
          options.AddPolicy(
            "read:data",
            policy => policy.Requirements.Add(
              new HasScopeRequirement("read:data", "https://dev-73urcdgst7428q7h.us.auth0.com/")
            )
          );

          options.AddPolicy(
            "write:data",
            policy => policy.Requirements.Add(
              new HasScopeRequirement("write:data", "https://dev-73urcdgst7428q7h.us.auth0.com/")
            )
          );
      });

builder.Services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
