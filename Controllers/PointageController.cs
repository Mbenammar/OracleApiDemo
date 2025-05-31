using Microsoft.AspNetCore.Mvc;
using OracleApiDemo.Services;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace OracleApiDemo.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    
    public class PointageController : ControllerBase
    {
        private readonly IOraclePointageService _pointageService;   
        private readonly ISqlServerPointageService _sqlServerPointageService;  

private readonly ILogger<PointageController> _logger;

        public PointageController(
            IOraclePointageService pointageService,
            ISqlServerPointageService sqlServerPointageService,
             ILogger<PointageController> logger)
        {
            _pointageService = pointageService;
            _sqlServerPointageService = sqlServerPointageService;
             _logger = logger;
        }
[Authorize(Roles = "UserPointage,Admin")]
        [HttpGet("test-access")]
        public IActionResult PointageEndpoint()
        {
            return Ok("Accessible par UserPointage ou Admin");
        }

        [HttpGet("Volet")]
        public async Task<IActionResult> GetPointage([FromQuery] string matricule, [FromQuery] int? mois)
        {
            var result = await _pointageService.GetPointageAsync(matricule, mois);
            return Ok(result);
        }


        [HttpGet("pointage-du-jour")]
public async Task<IActionResult> GetPointageDuJour([FromQuery] string matricule, [FromQuery] string datepointage)
{
    try
    {
    var result = await _pointageService.GetPointageDuJourAsync(matricule, datepointage);
   if (result == null || !result.Any())
        {
            return NotFound(new { message = "Aucun pointage trouvé pour les critères fournis." });
        }

        return Ok(result);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erreur lors de la récupération du pointage du jour.");
        return StatusCode(500, new { message = "Une erreur est survenue lors de la récupération du pointage du jour." });
    }
}
[HttpGet("semaine")]
public async Task<IActionResult> GetPointageSemaine([FromQuery] string matricule)
{
    try
    {
        var result = await _pointageService.GetPointageSemaineAsync(matricule);
        if (!result.Any())
            return NotFound(new { message = "Aucun pointage semaine trouvé." });

        return Ok(result);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erreur lors de la récupération du pointage semaine.");
        return StatusCode(500, new { message = "Erreur serveur lors du chargement du pointage semaine." });
    }
}
[HttpGet("autorisations")]
public async Task<IActionResult> GetAutorisations([FromQuery] string matricule, [FromQuery] string mois, [FromQuery] string? annee = null)
{
    try
    {
    if (string.IsNullOrEmpty(annee))
    {
        annee = DateTime.Now.Year.ToString();
    }
    
        var result = await _pointageService.GetAutorisationsAsync(matricule , mois, annee);

        if (result == null || !result.Any())
            return NotFound(new { message = "Aucune autorisation trouvée pour les critères fournis." });

        return Ok(result);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erreur lors de la récupération des autorisations.");
        return StatusCode(500, new { message = "Erreur serveur lors de la récupération des autorisations." });
    }
}

[HttpGet("retards")]
public async Task<IActionResult> GetRetards([FromQuery] string matricule, [FromQuery] string mois)
{
    if (string.IsNullOrEmpty(matricule) || string.IsNullOrEmpty(mois))
        return BadRequest("Matricule et mois sont requis.");

    try
    {
        
        var retards = await _pointageService.GetRetardsAsync(matricule, mois);
        if (!retards.Any())
            return Ok(new { message = "Aucun retard trouvé pour les critères fournis." });

        return Ok(retards);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erreur lors de la récupération des retards.");
        return StatusCode(500, new { message = "Erreur lors de la récupération des retards." });
    }
}

[HttpGet("nonautorises")]
public async Task<IActionResult> GetNonAutorises([FromQuery] string matricule, [FromQuery] string mois, [FromQuery] string? annee = null)
{
    

    try
    {
        if (string.IsNullOrEmpty(matricule) || string.IsNullOrEmpty(mois))
        return BadRequest("Matricule et mois sont requis.");

        if (string.IsNullOrEmpty(annee))
    {
        annee = DateTime.Now.Year.ToString();
    }
        var result = await _pointageService.GetNonAutorisesAsync(matricule, mois, annee);
        return Ok(result);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erreur lors de la récupération des pointages non autorisés.");
        return StatusCode(500, "Erreur interne.");
    }
}

[HttpGet("sanctions")]
public async Task<IActionResult> GetSanctions([FromQuery] string matricule, [FromQuery] string? annee = null)
{
    

    try
    {
        if (string.IsNullOrEmpty(matricule))
        return BadRequest("Matricule est  requis.");

        if (string.IsNullOrEmpty(annee))
    {
        annee = DateTime.Now.Year.ToString();
    }
        var result = await _pointageService.GetSanctionsAsync(matricule, annee);
        return Ok(result);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erreur lors de la récupération des sanctions.");
        return StatusCode(500, "Erreur interne.");
    }
}

/*************************pointyage distant*************************/

[HttpGet("pointage-distant")]
    public async Task<IActionResult> GetPointageDistant(
        [FromQuery] string matricule,
        [FromQuery] string tpers,
        [FromQuery] string datedeb)
    {
        if (string.IsNullOrWhiteSpace(matricule) || string.IsNullOrWhiteSpace(tpers) || string.IsNullOrWhiteSpace(datedeb))
        {
            return BadRequest("Les paramètres matricule, tpers et datedeb sont obligatoires.");
        }

        try
        {
            var result = await _sqlServerPointageService.GetPointageDistantAsync(matricule, tpers, datedeb);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du pointage distant.");
            return StatusCode(500, "Erreur interne du serveur.");
        }
    }

/******** fin pointage distant*******************/

    }
}
