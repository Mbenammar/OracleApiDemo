using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OracleApiDemo.Services;

namespace OracleApiDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PersonnelsController : ControllerBase
    {
        private readonly OraclePersonnelService _service;
        private readonly ILogger<PersonnelsController> _logger; // ajout
        public PersonnelsController(OraclePersonnelService service, ILogger<PersonnelsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [Authorize(Roles = "UserPointage,Admin")]
        [HttpGet("test-access")]
        public IActionResult PointageEndpoint()
        {
            return Ok("Accessible par UserPointage ou Admin");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            _logger.LogInformation($"Recherche du personnel avec ID : {id}");

        var personnel = await _service.GetByIdAsync(id);
        if (personnel == null)
        {
            _logger.LogWarning($"Personnel avec ID {id} non trouvé");
            return NotFound();
        }

        _logger.LogInformation($"Personnel avec ID {id} trouvé");
        return Ok(personnel);
        }
[Authorize] // Cette route nécessite un token valide
        [HttpGet]
public async Task<IActionResult> GetAll()
{
    _logger.LogInformation("Début de la récupération de tous les personnels");

        var personnels = await _service.GetAllAsync();

        _logger.LogInformation($"Nombre de personnels récupérés : {personnels.Count}");

    return Ok(personnels);
}

[HttpGet("affectation/{code}")]
public async Task<IActionResult> GetByCodeAffection(string code)
{
    var personnels = await _service.GetByCodeAffectionAsync(code);
    if (personnels == null || personnels.Count == 0)
        return NotFound("Aucun personnel trouvé pour ce code d'affectation.");

    return Ok(personnels);
}

[HttpGet("{id}/conge")]
public async Task<IActionResult> GetConge(string id)
{
    _logger.LogInformation($"Récupération des congés pour {id}");

    var conge = await _service.GetCongeAsync(id);
    _logger.LogInformation("Données de congé récupérées : {Conge}", System.Text.Json.JsonSerializer.Serialize(conge));
    if (conge == null)
        return NotFound($"Aucun congé trouvé pour le personnel avec l’ID {id}");

    return Ok(conge);
}

[HttpGet("{id}/conge/details")]
public async Task<IActionResult> GetDetailConge(string id)
{
    _logger.LogInformation($"Récupération des détails de congé pour {id}");

    var details = await _service.GetDetailCongeAsync(id);
    if (details == null || details.Count == 0)
    {
        _logger.LogWarning($"Aucun détail de congé trouvé pour {id}");
        return NotFound("Aucun détail de congé trouvé.");
    }

    _logger.LogInformation("Détails de congé récupérés : {@Details}", details);
    return Ok(details);
}
[HttpGet("details-conge")]
public async Task<IActionResult> GetDetailConge([FromQuery] CongeFilter filter)
{
    _logger.LogInformation($"Récupération des détails congés avec filtre: {System.Text.Json.JsonSerializer.Serialize(filter)}");
    
    var result = await _service.GetDetailCongeAsync(filter);
    if (result == null || !result.Any())
        return NotFound("Aucun congé trouvé pour les critères donnés.");

    return Ok(result);
}



    }
}
