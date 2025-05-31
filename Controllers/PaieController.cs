using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OracleApiDemo.Models;
using OracleApiDemo.Services;

namespace OracleApiDemo.Controllers
{
    [ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class PaieController : ControllerBase
{
    private readonly IOraclePaieService _paieService;

    public PaieController(IOraclePaieService paieService)
    {
        _paieService = paieService;
    }
[Authorize(Roles = "Admin")]
[HttpGet("admin-only")]
public IActionResult AdminOnlyEndpoint() => Ok("Accessible uniquement par l'admin");
[Authorize(Roles = "Admin")]
    [HttpGet("volet-paie")]
    public async Task<IActionResult> GetVoletPaie([FromQuery] string? matricule,[FromQuery] int? mois,[FromQuery] int? annee,[FromQuery] string? codeRubrique)
    {
        var result = await _paieService.GetVoletPaieAsync(matricule, mois, annee,codeRubrique);
        return Ok(result);
    }
}

}