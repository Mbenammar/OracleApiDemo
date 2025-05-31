public interface ISqlServerPointageService
{
    Task<IEnumerable<PointageDistantDto>> GetPointageDistantAsync(string matricule, string tpers, string datedeb);
}