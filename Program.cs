using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OracleApiDemo.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 🔐 Clé secrète (à déplacer dans appsettings.json ou un gestionnaire de secrets en prod)
var jwtKey = "CestUneCleSecreteUltraConfidentielleEtLongue";

// ✅ CONFIGURATION AUTHENTIFICATION JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// 🔧 Services de l’API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🔧 Ton service Oracle

builder.Services.AddScoped<IOraclePointageService, OraclePointageService>();
builder.Services.AddScoped<IOraclePaieService, OraclePaieService>();
builder.Services.AddSingleton<OracleApiDemo.Services.OraclePersonnelService>();
builder.Services.AddScoped<ISqlServerPointageService, SqlServerPointageService>();


var app = builder.Build();

//  Swagger en dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ✅ Middleware d’authentification/autorisation
app.UseAuthentication();    // << obligatoire AVANT UseAuthorization
app.UseAuthorization();

app.MapControllers(); // Active les contrôleurs comme PersonnelsController

app.Run();
