using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ----------------- Configuration -----------------
var keycloakConfig = builder.Configuration.GetSection("Keycloak");
var keycloakBaseUrl = keycloakConfig["BaseUrl"]?.TrimEnd('/') ?? throw new Exception("Keycloak BaseUrl missing");
var keycloakRealm = keycloakConfig["Realm"] ?? throw new Exception("Keycloak Realm missing");
var clientId = keycloakConfig["FrontendClientId"] ?? throw new Exception("FrontendClientId missing");
var requireHttps = bool.TryParse(keycloakConfig["RequireHttpsMetadata"], out var rqm) && rqm;

// ----------------- CORS -----------------
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ----------------- JWT Authentication -----------------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"{keycloakBaseUrl}/realms/{keycloakRealm}";
        options.Audience = clientId;
        options.RequireHttpsMetadata = requireHttps;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"{keycloakBaseUrl}/realms/{keycloakRealm}",
            ValidateAudience = true,
            ValidAudience = clientId,
            ValidateLifetime = true,
            NameClaimType = "preferred_username",
            RoleClaimType = ClaimTypes.Role
        };

        // Map roles from JWT token
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = ctx =>
            {
                var identity = (ClaimsIdentity)ctx.Principal!.Identity!;
                var token = ctx.SecurityToken as JwtSecurityToken;

                if (token?.Payload.TryGetValue("realm_access", out var raObj) == true
                    && raObj is IDictionary<string, object> ra
                    && ra.TryGetValue("roles", out var rolesObj)
                    && rolesObj is IEnumerable<object> roles)
                {
                    foreach (var role in roles)
                        identity.AddClaim(new Claim(ClaimTypes.Role, role.ToString()!));
                }
                else
                {
                    Console.WriteLine("No roles found in realm_access!");
                }

                return Task.CompletedTask;
            }
        };
    });

// ----------------- Authorization Policies -----------------
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireSystemAdmin", p => p.RequireRole("system_admin"));
});

// ----------------- Swagger -----------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// 1) Keycloak config for frontend
app.MapGet("/auth/config", () =>
{
    var issuer = $"{keycloakBaseUrl}/realms/{keycloakRealm}";
    return Results.Ok(new
    {
        issuer,
        authorizationEndpoint = $"{issuer}/protocol/openid-connect/auth",
        tokenEndpoint = $"{issuer}/protocol/openid-connect/token",
        jwksUri = $"{issuer}/protocol/openid-connect/certs",
        clientId
    });
});

// 2) User info endpoint
app.MapGet("/me", (ClaimsPrincipal user) =>
{
    if (user?.Identity?.IsAuthenticated != true) return Results.Unauthorized();

    var username = user.Identity?.Name ?? user.FindFirstValue("preferred_username");
    var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).Distinct().ToArray();

    return Results.Ok(new
    {
        username,
        roles,
        access = new
        {
            canManageUsers = roles.Contains("system_admin")
        }
    });
}).RequireAuthorization();

// 3) Admin-only endpoint
app.MapGet("/admin/only", () => Results.Ok("Hello Admin"))
   .RequireAuthorization("RequireSystemAdmin");

app.Run();
