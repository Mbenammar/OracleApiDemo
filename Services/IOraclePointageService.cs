using OracleApiDemo.Models;

namespace OracleApiDemo.Services
{
    public interface IOraclePointageService
    {
        Task<IEnumerable<PointageDto>> GetPointageAsync(string matricule, int? mois);
        Task<IEnumerable<PointageDuJourDto>> GetPointageDuJourAsync(string matricule, string datepointage);
        Task<IEnumerable<PointageSemaineDto>> GetPointageSemaineAsync(string matricule); // string
        Task<IEnumerable<AutorisationDto>> GetAutorisationsAsync(string matricule, string mois, string? annee = null);
        Task<IEnumerable<NonAutoriseDto>> GetNonAutorisesAsync(string matricule, string mois, string? annee = null);
        Task<IEnumerable<RetardDto>> GetRetardsAsync(string matricule, string mois);
        Task<IEnumerable<SanctionDto>> GetSanctionsAsync(string matricule, string? annee = null);

    }
    
}
