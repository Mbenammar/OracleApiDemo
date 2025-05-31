using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OracleApiDemo.Models; // ⬅️ Ajoute ceci
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OracleApiDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // ⚠️ À mettre dans appsettings.json ou une variable d’environnement en production
        private readonly string jwtKey = "CestUneCleSecreteUltraConfidentielleEtLongue";

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
                        // ⚠️ Simulation (remplace par une vraie vérification plus tard)
                if (model.Username == "admin" && model.Password == "admin123")
                {
                    var token = GenerateJwtToken(model.Username, "Admin");
                    return Ok(new { token });
                }
                else if (model.Username == "user1" && model.Password == "user123")
                {
                    var token = GenerateJwtToken(model.Username, "UserPointage");
                    return Ok(new { token });
                }

                return Unauthorized("Nom d’utilisateur ou mot de passe incorrect");
        }
/* private string GenerateJwtToken(string username)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwtKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, "Admin") // ou autre rôle selon ton besoin
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        */
private string GenerateJwtToken(string username, string role)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(jwtKey);

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role) // <-- ici, rôle dynamique
        }),
        Expires = DateTime.UtcNow.AddHours(1),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}

    }

public class LoginModel
{
    public string Username { get; set; }
    public string Password { get; set; }
}
}
