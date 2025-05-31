using OracleApiDemo.Models;

namespace OracleApiDemo.Services
{
    public interface IOraclePaieService
{
    Task<IEnumerable<VoletPaieDto>> GetVoletPaieAsync(string? matricule,int? mois,int? annee,string? codeRubrique);
}

}